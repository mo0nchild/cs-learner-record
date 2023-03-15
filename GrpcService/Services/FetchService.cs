using Grpc.Core;
using GrpcService;

namespace GrpcService.Services
{
    public class FetchService : FetchData.FetchDataBase
    {
        public const string file = "C:\\Users\\Byter\\Documents\\Projects\\1C Enterprise\\University";
        private readonly ILogger<FetchService> _logger;
        public FetchService(ILogger<FetchService> logger) => this._logger = logger;

        public override Task<ProfileData> GetProfileData(RequestData request, ServerCallContext context)
        {
            var com_connector = new V83.COMConnector();
            var result = com_connector.Connect($"File=\'{FetchService.file}\';Usr=\'�������������\';pwd=\'\';");

            var query = result.NewObject("������");
            query.����� = @$"
                �������

                ��������������.��� ��� ���,
                ������������������.�������������.������������ ��� �������������,
                ��������������.������� ��� �������,
                ��������������.��� ��� ���,
                ��������������.�������� ��� ��������,

                ������������������.������.������������ ��� �����������������,
                ������������������.���������.������������ ��� ��������������,
                ������������������.�������������.������������ ��� �������������,
                ������������������.�������������.������������ ��� �������,
                ������������������.������.������������ ��� ������,
                ������������������.����.������������ ��� ����,
                ������������������.�����������.�����������������.������������ ��� ����������,
                ������������������.���������.������������ ��� ���������,
                ������������������.�������������.������������ ��� �������������

            ��
                ���������������.������������������ ��� ������������������
            ���������� ���������� ����������.�������������� ��� ��������������
                �� (������������������.�������������� = ��������������.������)
            ���������� ���������� ����������.���������������������� ��� ����������������������
                �� (����������������������.������ = ������������������.���������)
            ���    ����������������������.������������ = ""�������� ���������""
                � ������������������.�������������.������������ = ""{request.StudentGradeBook}""";

            var refer = query.���������().�������();
            var profile = new ProfileData();

            if (refer.��������� == true)
            {
                profile.Id = refer.���;
                profile.FirstName = refer.���;
                profile.LastName = refer.��������;
                profile.Group = refer.������;
                profile.TuitionType = refer.�����������������;
                profile.TuitionForm = refer.�������������;
                profile.TrainingLevel = refer.����������;
                profile.Speciality = refer.�������������;
                profile.Specialization =  DBNull.Value.Equals(refer.�������) ? "" : refer.�������;
                profile.Course = refer.����;
                profile.StudentStatus = refer.��������������;
                profile.Patronymic = refer.��������;

                this._logger.LogInformation((string)refer.���);
                this._logger.LogInformation((string)refer.���);
                this._logger.LogInformation((string)refer.��������);
                this._logger.LogInformation((string)refer.������);
                this._logger.LogInformation((string)refer.�����������������);
                this._logger.LogInformation((string)refer.�������������);
                this._logger.LogInformation((string)refer.����������);
                this._logger.LogInformation((string)refer.�������������);
                this._logger.LogInformation((string)refer.�������);
                this._logger.LogInformation((string)refer.����);
                this._logger.LogInformation((string)refer.��������������);
            }
            return Task.FromResult(profile);
        }

        public override Task<ProfileStatement> GetProfileStatement(RequestData request, ServerCallContext context)
        {
            var com_connector = new V83.COMConnector();
            var result = com_connector.Connect($"File=\'{FetchService.file}\';Usr=\'�������������\';pwd=\'\';");

            var query = result.NewObject("������");
            query.����� = @$"
                    ������� 
                    ����������, 
                    �������������� ��� ������, 
                    �����������, 
                    �������, 
                    �����������, 
                    �����������, 
                    �������������.������.��� ��� ����������������,
                    �������������.������.������������ ��� �������������
                    �� ��������.���������
                    ���������� ����������  ��������.���������.������������������
                    �� ��������.���������.������ =  ��������.���������.������������������.������
                    ���������� ����������  ��������.���������.�������������
                    �� ��������.���������.������ =  ��������.���������.�������������.������
                    ��� �������������.������.������������ = ""{request.StudentGradeBook}""
                    ����������� �� �������������� ����
            ";

            var refer = query.���������().�������();

            var profileStatement = new ProfileStatement();

            IList<ProfileStatement.Types.StatementInfo> statements = new List<ProfileStatement.Types.StatementInfo>();
            while (refer.��������� == true)
            {
                ProfileStatement.Types.StatementInfo statement = new ProfileStatement.Types.StatementInfo();
                statement.Subject = refer.����������.������������;
                statement.TeacherId = refer.����������������;
                statement.SemestrNumber = refer.������.������������;
                statement.TeacherFullName = refer.�������������;
                statement.Assessment = refer.�������.������������;
                statement.TypeAttestation = refer.�����������.������������;
                statement.Date = Convert.ToString(refer.�����������);
                //Timestamp.FromDateTime(DateTime.UtcNow)

                Console.WriteLine(refer.����������.������������);
                statements.Add(statement);
            }
            return Task.FromResult(profileStatement);
        }

    }
}



