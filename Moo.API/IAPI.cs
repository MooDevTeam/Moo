using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;
namespace Moo.API
{
    [ServiceContract]
    public interface IAPI
    {
        [OperationContract]
        string Login(string userName, string password);
        [OperationContract]
        void AddTrustedUser(string sToken, int userID);
        [OperationContract]
        void RemoveTrustedUser(string sToken, int userID);
        [OperationContract]
        List<BriefUserInfo> ListTrustedUser(string sToken);
        [OperationContract]
        BriefUserInfo GetUserByName(string sToken, string userName);
        [OperationContract]
        int CreateProblem(string sToken, string name, string type, string content);
        [OperationContract]
        int CreateTranditionalTestCase(string sToken, int problemID, byte[] input, byte[] answer, int timeLimit, int memoryLimit, int score);
        [OperationContract]
        int CreateSpecialJudgedTestCase(string sToken, int problemID, int judgerID, byte[] input, byte[] answer, int timeLimit, int memoryLimit);
    }
    [DataContract]
    public class BriefUserInfo
    {
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
