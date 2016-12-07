using System;

namespace Rest.Net.Exceptions
{
    public class DidNotGetAroundToItOpenGithubIssueException : NotImplementedException
    {
        public override string Message => "Dear developer, Sorry, I did not get around to implement this method. I will do my best to get around to it. In the meanwhile you are welcome to open a Github issue.";
    }
}
