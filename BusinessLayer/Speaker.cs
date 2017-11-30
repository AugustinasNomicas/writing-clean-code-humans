using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessLayer;

namespace BusinessLayer
{
    public class Speaker
    {
        private readonly List<string> OffTopics = new List<string>() { "Cobol", "Punch Cards", "Commodore", "VBScript" };
        private readonly List<string> BlockedDomains = new List<string>() { "aol.com", "hotmail.com", "prodigy.com", "CompuServe.com" };
        private readonly List<string> TrustedEmployers = new List<string>() { "Microsoft", "Google", "Fog Creek Software", "37Signals" };

        private List<(int low, int high, int fee)> RegistrationFees = new List<(int low, int high, int fee)>
            {
                (int.MinValue, 1, 500),
                (2, 3, 250),
                (4, 5, 100),
                (6, 9, 50),
                (10, int.MaxValue, 0)
            };

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int? Exp { get; set; }
        public bool HasBlog { get; set; }
        public string BlogURL { get; set; }
        public WebBrowser Browser { get; set; }
        public List<string> Certifications { get; set; }
        public string Employer { get; set; }
        public int RegistrationFee { get; set; }
        public List<BusinessLayer.Session> Sessions { get; set; }

        public int? Register(IRepository repository)
        {
            ValidateRegistration();

            RegistrationFee = CalculateRegistrationFee();
            return repository.SaveSpeaker(this);
        }

        private void ValidateRegistration()
        {
            ValidateForEmptyValues();

            var speakerAppearsQualified = AppearsExceptional() || !ObviuosRedFlags();

            if (!speakerAppearsQualified)
            {
                throw new SpeakerDoesntMeetRequirementsException("Speaker doesn't meet our abitrary and capricious standards.");
            }

            ApproveSessions();

            if (Sessions.All(s => !s.Approved))
            {
                throw new NoSessionsApprovedException("No sessions approved.");
            }
        }

        private int CalculateRegistrationFee() => RegistrationFees.Single(f => Exp >= f.low && Exp <= f.high).fee;

        private void ValidateForEmptyValues()
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                throw new ArgumentNullException("First Name is required");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                throw new ArgumentNullException("Last name is required.");
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                throw new ArgumentNullException("Email is required.");
            }

            if (!Sessions.Any())
            {
                throw new ArgumentException("Can't register speaker with no sessions to present.");
            }
        }

        private bool AppearsExceptional()
        {
            return Exp > 10
                || HasBlog
                || Certifications.Count() > 3
                || TrustedEmployers.Contains(Employer);
        }

        private bool ObviuosRedFlags() => EmailFromBlockedDomain() || IsOldInternetExplorer();
        private bool EmailFromBlockedDomain() => BlockedDomains.Contains(Email.Split('@').Last());
        private bool IsOldInternetExplorer() => Browser.Name == WebBrowser.BrowserName.InternetExplorer && Browser.MajorVersion < 9;


        private void ApproveSessions()
        {
            foreach (var session in Sessions)
            {
                session.Approved = !SessionContainsOffTopics(session);
            }
        }

        private bool SessionContainsOffTopics(Session session) => OffTopics.Any(ot => session.Title.Contains(ot) || session.Description.Contains(ot));

        #region Custom Exceptions
        public class SpeakerDoesntMeetRequirementsException : Exception
        {
            public SpeakerDoesntMeetRequirementsException(string message)
                : base(message)
            {
            }

            public SpeakerDoesntMeetRequirementsException(string format, params object[] args)
                : base(string.Format(format, args)) { }
        }

        public class NoSessionsApprovedException : Exception
        {
            public NoSessionsApprovedException(string message)
                : base(message)
            {
            }
        }
        #endregion
    }
}