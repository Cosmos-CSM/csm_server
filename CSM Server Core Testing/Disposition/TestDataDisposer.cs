using CSM_Server_Core_Testing.Disposition.Abstractions.Bases;

namespace CSM_Server_Core_Testing.Disposition;

/// <inheritdoc cref="TestDataDisposerBase"/>
public class TestDataDisposer 
    : TestDataDisposerBase {

    /// <inheritdoc/>
    public TestDataDisposer(params DatabaseFactory[] factories) 
        : base(factories) {
    }
}
