// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace Aiel.Internal;

// IMPORTANT: All diagnostic descriptor IDs must start with the canonical "AIEL" prefix.
// Never use any other prefix (e.g. TRAF, TRSG, TRMD). See GitHub issue #7.
public static partial class DiagnosticRuleIDs
{
    public const String AIEL00001_RootDependencyRequiredId = "AIEL00001";
    public const String AIEL00002_ErrorTypesMustHaveSingleStringConstructorId = "AIEL00002";
    public const String AIEL00003_PreferResultHttpClientExtensionsId = "AIEL00003";
    public const String AIEL00004_AmbiguousProjectTypeDiagnosticId = "AIEL00004";
    public const String AIEL00005_MultipleDispatchCallsInMethodId = "AIEL00005";
    public const String AIEL00006_ActionHasNoAuthorizationStoryId = "AIEL00006";
    public const String AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId = "AIEL00007";
    public const String AIEL00008_UseAielEventIdsId = "AIEL00008";
    public const String AIEL00009_MissingEventIdParameterId = "AIEL00009";
    public const String AIEL00010_MissingEventIdInMessageId = "AIEL00010";
    public const String AIEL00011_NoDirectILoggerCallsId = "AIEL00011";
    public const String AIEL00012_EventIdMismatchId = "AIEL00012";
    public const String AIEL00013_MustBePartialRecordTypeId = "AIEL00013";
    public const String AIEL00014_MustNotUsePositionalRecordSyntaxId = "AIEL00014";
    public const String AIEL00015_MustImplementMatchingInterfaceId = "AIEL00015";
    public const String AIEL00016_MustNotDeclareValueMemberId = "AIEL00016";
    public const String AIEL00017_MustNotDeclareInstanceConstructorsId = "AIEL00017";
    public const String AIEL00018_BackingTypeUnsupportedId = "AIEL00018";
    public const String AIEL00019_NoNmeaMessageTypesDiscoveredId = "AIEL00019";
}
