using Bimss.Application.Auditing;
using Bimss.Domain.Auditing;
using Bimss.Domain.Exceptions;
using Bimss.Domain.Membership;

namespace Bimss.Application.Membership;

// Direct self-service edit — no officer review — for the one profile area
// docs/DATA_DICTIONARY.md's confirmed decision (2026-08-14) allows it for:
// "Self-service direct edit (no officer approval) is limited to contact
// information only (phone, email, mailing address). All other profile
// fields ... go through the officer review/approval workflow"
// (BIMSS-041/042/043). Present and Permanent addresses are both treated as
// "mailing address" here — the data dictionary doesn't distinguish between
// them for this rule, and neither carries the identity/employment
// implications that keep name/civil-status/employment changes in the
// approval workflow.
public sealed class MemberContactSelfServiceUpdateService(IMemberRepository memberRepository, IAuditLogger auditLogger)
{
    public async Task UpdateAsync(
        Guid memberId,
        Guid actorUserId,
        string? landline,
        string mobileNumber,
        string email,
        string? presentAddress,
        string? permanentAddress,
        CancellationToken cancellationToken = default)
    {
        if (!await memberRepository.ExistsAsync(memberId, cancellationToken))
        {
            throw new NotFoundException("Member", memberId);
        }

        var contact = await memberRepository.GetTrackedContactByMemberIdAsync(memberId, cancellationToken);
        if (contact is not null)
        {
            contact.UpdateDetails(landline, mobileNumber, email);
        }
        else
        {
            await memberRepository.AddContactAsync(
                new MemberContact(Guid.NewGuid(), memberId, landline, mobileNumber, email), cancellationToken);
        }

        var addresses = await memberRepository.GetTrackedAddressesByMemberIdAsync(memberId, cancellationToken);
        await UpsertAddressAsync(memberId, addresses, MemberAddressType.Present, presentAddress, cancellationToken);
        await UpsertAddressAsync(memberId, addresses, MemberAddressType.Permanent, permanentAddress, cancellationToken);

        await memberRepository.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            new AuditEntry(actorUserId, "Member.UpdateContactInfo", "Member", memberId.ToString(), AuditResult.Success),
            cancellationToken);
    }

    // A blank value leaves an existing address untouched rather than
    // clearing it — MemberAddress.AddressLine has no "unset" concept (its
    // domain constructor/mutator both require non-blank text), and this
    // form has no separate "remove address" action.
    private async Task UpsertAddressAsync(
        Guid memberId,
        IReadOnlyList<MemberAddress> addresses,
        MemberAddressType addressType,
        string? addressLine,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
        {
            return;
        }

        var existing = addresses.FirstOrDefault(address => address.AddressType == addressType);
        if (existing is not null)
        {
            existing.UpdateAddressLine(addressLine);
        }
        else
        {
            await memberRepository.AddAddressAsync(new MemberAddress(Guid.NewGuid(), memberId, addressType, addressLine), cancellationToken);
        }
    }
}
