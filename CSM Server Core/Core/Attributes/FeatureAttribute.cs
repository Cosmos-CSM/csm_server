namespace CSM_Server_Core.Core.Attributes;

/// <summary>
///     Attribute to determine the Feature representation for a server service provider.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class FeatureAttribute
    : Attribute {

    /// <summary>
    ///     Feature specified name
    /// </summary>
    public readonly string Feature;

    /// <summary>
    ///     Creates a new <see cref="FeatureAttribute"/> instance to handle the Feature context for authentication.
    /// </summary>
    /// <param name="featureName">
    ///     Feature name.
    /// </param>
    public FeatureAttribute(string featureName) {
        Feature = featureName;
    }
}
