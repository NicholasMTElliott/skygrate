namespace Skyward.Skygrate.Core
{
    public enum MigrationStatus
    {
        Unknown = 0,
        ValidChain,
        InvalidWithin,
        InvalidAfter,
        Pending,
        Changed
    }
}
