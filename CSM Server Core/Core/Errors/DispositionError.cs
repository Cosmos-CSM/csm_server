using CSM_Server_Core.Abstractions.Bases;

namespace CSM_Server_Core.Core.Errors;

/// <summary>
///     <see langword="enum"/> implementation.
///     
///     <para>
///         Defines the available possible {Situations} for <see cref="DispositionError"/> exception invokation.
///     </para>
/// </summary>
public enum XDispositionSituations {
    /// <summary>
    ///     
    /// </summary>
    WRONG_TOKEN,
}

/// <summary>
///     {exception} class from <see cref="BException{XDispositionSituation}"/>.
///     
///     <para>
///         Defines an exception object thrown at {Disposition} data process. 
///     </para>
/// </summary>
public class DispositionError
    : ServerErrorBase<XDispositionSituations> {

    /// <summary>
    ///     Creates a new <see cref="DispositionError"/> instance.
    /// </summary>
    /// <param name="situation"></param>
    /// <exception cref="ArgumentException"></exception>
    public DispositionError(XDispositionSituations situation)
        : base($"Data disposition process exception", situation) {
    }

    protected override Dictionary<XDispositionSituations, string> BuildAdviseContext() {

        return new Dictionary<XDispositionSituations, string> {
            { XDispositionSituations.WRONG_TOKEN, "Wrong {CSMDisposition} header value format" }
        };
    }
}
