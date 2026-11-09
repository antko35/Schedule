using Xunit;

// Hangfire's logging provider is process-global; concurrent API factories can
// otherwise retain a logger from an already disposed host during restart tests.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
