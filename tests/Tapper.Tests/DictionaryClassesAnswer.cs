// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY Tapper.Test
// </auto-generated>
#nullable enable
using System;
using System.Collections.Generic;
using Tapper.Tests.SourceTypes;

namespace Tapper.Tests;

public static class DictionaryTypeTranspilationAnswer
{
    public static Dictionary<string, string> Dict = new();

    static DictionaryTypeTranspilationAnswer()
    {
        Dict[nameof(ClassIncludeDictionaryFieldDictionaryintstring)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryFieldDictionaryintstring */
export type ClassIncludeDictionaryFieldDictionaryintstring = {
  /** Transpiled from System.Collections.Generic.Dictionary<int, string> */
  FieldOfDictionaryintstring: { [key: number]: string };
};
";
        Dict[nameof(ClassIncludeDictionaryFieldIDictionaryfloatGuid)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryFieldIDictionaryfloatGuid */
export type ClassIncludeDictionaryFieldIDictionaryfloatGuid = {
  /** Transpiled from System.Collections.Generic.IDictionary<float, System.Guid> */
  FieldOfIDictionaryfloatGuid: { [key: number]: string };
};
";
        Dict[nameof(ClassIncludeDictionaryFieldIReadOnlyDictionarystringDateTime)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryFieldIReadOnlyDictionarystringDateTime */
export type ClassIncludeDictionaryFieldIReadOnlyDictionarystringDateTime = {
  /** Transpiled from System.Collections.Generic.IReadOnlyDictionary<string, System.DateTime> */
  FieldOfIReadOnlyDictionarystringDateTime: { [key: string]: (Date | string) };
};
";

        Dict[nameof(ClassIncludeDictionaryPropertyDictionaryintstring)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryPropertyDictionaryintstring */
export type ClassIncludeDictionaryPropertyDictionaryintstring = {
  /** Transpiled from System.Collections.Generic.Dictionary<int, string> */
  PropertyOfDictionaryintstring: { [key: number]: string };
};
";
        Dict[nameof(ClassIncludeDictionaryPropertyIDictionaryfloatGuid)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryPropertyIDictionaryfloatGuid */
export type ClassIncludeDictionaryPropertyIDictionaryfloatGuid = {
  /** Transpiled from System.Collections.Generic.IDictionary<float, System.Guid> */
  PropertyOfIDictionaryfloatGuid: { [key: number]: string };
};
";
        Dict[nameof(ClassIncludeDictionaryPropertyIReadOnlyDictionarystringDateTime)] = @"/** Transpiled from Tapper.Tests.SourceTypes.ClassIncludeDictionaryPropertyIReadOnlyDictionarystringDateTime */
export type ClassIncludeDictionaryPropertyIReadOnlyDictionarystringDateTime = {
  /** Transpiled from System.Collections.Generic.IReadOnlyDictionary<string, System.DateTime> */
  PropertyOfIReadOnlyDictionarystringDateTime: { [key: string]: (Date | string) };
};
";
    }
}
