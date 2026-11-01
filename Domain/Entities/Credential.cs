namespace TaikoLocalServer.Domain.Entities
{
    public partial class Credential
    {
        public uint Baid { get; set; }
        public string Password { get; set; } = null!;
        public string Salt { get; set; } = null!;

        /// <summary>2-step verification: the authenticator key (base32); null when it's off.</summary>
        public string? TotpSecret { get; set; }
        /// <summary>The last 30-second step a code was accepted for, so a code works only once.</summary>
        public long TotpLastStep { get; set; }
        /// <summary>SHA-256 hex of the unused recovery codes, one per line.</summary>
        public string RecoveryCodes { get; set; } = string.Empty;

        public virtual UserDatum? Ba { get; set; }
    }
}