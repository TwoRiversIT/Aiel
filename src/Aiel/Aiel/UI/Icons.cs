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

using Aiel.Reflection;

namespace Aiel.UI;

public static class Icons
{
    public static class Duotone
    {
        public const String Prefix = "fa-duotone";

        public const String Actions = Prefix + " -person-digging fa-fw";
        public const String Add = Prefix + " -plus fa-fw";
        public const String Address = Prefix + " -location-smile fa-fw";
        public const String Admin = Prefix + " -user-crown fa-fw";
        public const String Application = Prefix + " -browser fa-fw";
        public const String Approve = Prefix + " -check fa-fw";
        public const String Archive = Prefix + " -box-archive fa-fw";
        public const String Back = Prefix + " -left fa-fw";
        public const String Bulletin = Prefix + " -memo-pad fa-fw";
        public const String Calendar = Prefix + " -calendars fa-fw";
        public const String Cancel = Prefix + " -times fa-fw";
        public const String CandidateSupervision = "fa-light fa-user-friends fa-fw";
        public const String Certificate = Prefix + " -file-certificate fa-fw";
        public const String Changed = Prefix + " -rotate-exclamation fa-fw";
        public const String CheckboxChecked = "fa-regular fa-check-square fa-fw";
        public const String CheckboxNull = "fa-regular fa-square fa-fw";
        public const String CheckboxUnchecked = "fa-regular fa-times-square fa-fw";
        public const String Checked = Prefix + " -check-square fa-fw";
        public const String Claims = Prefix + " -ballot-check fa-fw";
        public const String ClinicalSupervision = Prefix + " -user-friends fa-fw";
        public const String Close = Prefix + " -xmark fa-fw";
        public const String Comment = Prefix + " -comment-alt-lines fa-fw";
        public const String ContactInformation = Prefix + " -id-card fa-fw";
        public const String ContactUs = Prefix + " -message-text fa-fw";
        public const String ContinuingEducation = Prefix + " -graduation-cap fa-fw";
        public const String CounsellingApproach = Prefix + " -ballot-check fa-fw";
        public const String CounsellingIssue = "fa-solid fa-ballot-check fa-fw";
        public const String CounsellingType = "fa-light fa-ballot-check fa-fw";
        public const String CriminalRecordCheck = Prefix + " -badge-sheriff fa-fw";
        public const String Crop = Prefix + " -crop fa-fw";
        public const String Currency = Prefix + " -dollar-sign fa-fw";
        public const String CurrencyNull = "fa-light fa-dollar-sign fa-fw";
        public const String CurrentEnrollment = Prefix + " -school fa-fw";
        public const String Dashboard = Prefix + " -tachometer-alt fa-fw";
        public const String Date = Prefix + " -calendar-check fa-fw";
        public const String DateNull = Prefix + " -calendar fa-fw";
        public const String DateTime = Prefix + " -clock fa-fw";
        public const String DateTimeNull = Prefix + " -clock fa-fw";
        public const String Day = Prefix + " -calendar-day fa-fw";
        public const String Decline = Prefix + " -times fa-fw";
        public const String Delete = Prefix + " -trash-can fa-fw";
        public const String DirectoryListing = Prefix + " -book-user fa-fw";
        public const String DirectoryPhoto = Prefix + " -portrait fa-fw";
        public const String Download = Prefix + " -download fa-fw";
        public const String Draft = Prefix + " -pen-ruler fa-fw";
        public const String Dues = Prefix + " -sack-dollar fa-fw";
        public const String Duration = Prefix + " -stopwatch fa-fw";
        public const String DurationNull = "fa-light fa-stopwatch fa-fw";
        public const String Edit = Prefix + " -pencil fa-fw";
        public const String Email = Prefix + " -at fa-fw";
        public const String Empty = Prefix + " -solid fa-empty-set fa-fw";
        public const String Excel = Prefix + " -file-xls fa-fw";
        public const String Execute = Prefix + " -transformer-bolt fa-fw";
        public const String ExternalLink = Prefix + " -external-link fa-fw";
        public const String File = Prefix + " -solid fa-file fa-fw";
        public const String Forward = Prefix + " -right fa-fw";
        public const String Home = Prefix + " -home fa-fw";
        public const String Image = Prefix + " -solid fa-file-image fa-fw";
        public const String Impersonate = Prefix + " -user-secret fa-fw";
        public const String Invoice = Prefix + " -file-invoice fa-fw";
        public const String Letter = Prefix + " -file-contract fa-fw";
        public const String LiabilityInsurance = Prefix + " -shield-quartered fa-fw";
        public const String Login = Prefix + " -person-to-portal fa-fw";
        public const String Logout = Prefix + " -person-from-portal fa-fw";
        public const String Magic = Prefix + " -wand-magic-sparkles fa-fw";
        public const String Mastercard = "fa-brand fa-cc-mastercard fa-fw";
        public const String Member = Prefix + " -user fa-fw";
        public const String MemberCard = Prefix + " -id-card-alt fa-fw";
        public const String MembershipChange = Prefix + " -person-booth fa-fw";
        public const String MembershipDues = Prefix + " -usd-circle fa-fw";
        public const String MembershipHistory = Prefix + " -user-clock fa-fw";
        public const String MembershipRenewal = Prefix + " -file-certificate fa-fw";
        public const String Minutes = Prefix + " -stopwatch fa-fw";
        public const String MoveDown = Prefix + " -arrow-down fa-fw";
        public const String MoveUp = Prefix + " -arrow-up fa-fw";
        public const String NameChange = Prefix + " -signature fa-fw";
        public const String Newsletter = Prefix + " -newspaper fa-fw";
        public const String Note = Prefix + " -comments-alt fa-fw";
        public const String Number = Prefix + " -hashtag fa-fw fa-fw";
        public const String Off = Prefix + " -toggle-off fa-fw";
        public const String On = Prefix + " -toggle-on fa-fw";
        public const String Password = Prefix + " -key fa-fw";
        public const String Payment = Prefix + " -receipt fa-fw";
        public const String PDF = Prefix + " -file-pdf fa-fw";
        public const String Phone = Prefix + " -phone fa-fw";
        public const String Pin = Prefix + " -thumb-tack fa-fw";
        public const String PostInvoice = Prefix + " -file-invoice fa-fw";
        public const String PowerPoint = Prefix + " -solid fa-file-powerpoint fa-fw";
        public const String Privacy = Prefix + " -shield-check fa-fw";
        public const String Profile = Prefix + " -user fa-fw";
        public const String Publish = Prefix + " -upload fa-fw";
        public const String RealignDesignation = Prefix + " -regular fa-user-doctor fa-fw";
        public const String Refresh = Prefix + " -arrows-rotate fa-fw";
        public const String Renewal = Prefix + " -award";
        public const String Report = Prefix + " -chart-waterfall fa-fw";
        public const String Requirement = Prefix + " -tag fa-fw";
        public const String Reset = Prefix + " -undo fa-fw";
        public const String Review = Prefix + " -thumbs-up fa-fw";
        public const String Roles = Prefix + " -users-rectangle fa-fw";
        public const String Save = Prefix + " -down-to-bracket fa-fw";
        public const String ScopeOfPractice = Prefix + " -split fa-fw";
        public const String Search = Prefix + " -search fa-fw";
        public const String Send = Prefix + " -paper-plane fa-fw";
        public const String Server = Prefix + " -server fa-fw";
        public const String Settings = Prefix + " -solid fa-wrench fa-fw";
        public const String Signature = Prefix + " -signature fa-fw";
        public const String SignIn = Prefix + " -sign-in fa-fw";
        public const String SignOut = Prefix + " -sign-out fa-fw";
        public const String SortAscending = Prefix + " -sort-down fa-fw";
        public const String SortDescending = Prefix + " -sort-up fa-fw";
        public const String SortUnspecified = Prefix + " -sort fa-fw";
        public const String Success = Prefix + " -checkmark fa-fw";
        public const String Sync = Prefix + " -arrow-right-arrow-left fa-fw";
        public const String Text = Prefix + " -solid fa-file-lines fa-fw";
        public const String Toast = Prefix + " -bread-loaf fa-fw";
        public const String Unchecked = Prefix + " -square fa-fw";
        public const String Undo = Prefix + " -undo fa-fw";
        public const String User = Prefix + " -user fa-fw";
        public const String Users = Prefix + " -users fa-fw";
        public const String Version = Prefix + " -duotone fa-code-commit fa-fw";
        public const String Video = Prefix + " -solid fa-file-video fa-fw";
        public const String View = Prefix + " -eye fa-fw";
        public const String Visa = "fa-brand fa-cc-visa fa-fw";
        public const String VulnerableSectorCheck = Prefix + " -child fa-fw";
        public const String Warning = Prefix + " -exclamation-triangle fa-fw";
        public const String Welcome = Prefix + " -handshake fa-fw";
        public const String Word = Prefix + " -solid fa-file-word fa-fw";
        public const String Year = Prefix + " -timeline fa-fw";
    }

    public static String[] GetAll()
        => typeof(Icons).GetConstants();
}
