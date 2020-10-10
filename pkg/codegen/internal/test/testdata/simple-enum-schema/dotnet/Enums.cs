// *** WARNING: this file was generated by test. ***
// *** Do not edit by hand unless you're certain you know what you are doing! ***

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Pulumi.Serialization;

namespace Pulumi.PlantProvider.Enums
{
    /// <summary>
    /// plant container colors
    /// </summary> 
    public enum ContainerColor
    {
        [EnumMember(Value = "red")]
        Red,
        [EnumMember(Value = "blue")]
        Blue,
        [EnumMember(Value = "yellow")]
        Yellow
    }

    /// <summary>
    /// plant container sizes
    /// </summary>    
    public enum ContainerSize
    {
        [EnumMember(Value = 4)]
        FourInch,
        [EnumMember(Value = 6)]
        SixInch,
        [Obsolete(@"Eight inch pots are no longer supported.")]
        [EnumMember(Value = 8)]
        EightInch
    }
}
