using System;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.CompositeWeb;
using Microsoft.Practices.CompositeWeb.Interfaces;
using Microsoft.Practices.CompositeWeb.Utility;
using Microsoft.Practices.ObjectBuilder;

using Chai.WorkflowManagment.CoreDomain;
using Chai.WorkflowManagment.CoreDomain.DataAccess;
using Chai.WorkflowManagment.CoreDomain.Admins;
using Chai.WorkflowManagment.CoreDomain.Users;
using Chai.WorkflowManagment.Services;
using Chai.WorkflowManagment.Shared.Navigation;


using System.Data;
using Chai.WorkflowManagment.CoreDomain.Setting;
using Chai.WorkflowManagment.CoreDomain.HRM;
using Chai.WorkflowManagment.CoreDomain.Request;

namespace Chai.WorkflowManagment.Modules.Setting
{
    public class SettingController : ControllerBase
    {
        private IWorkspace _workspace;

        [InjectionConstructor]
        public SettingController([ServiceDependency] IHttpContextLocatorService httpContextLocatorService, [ServiceDependency]INavigationService navigationService)
            : base(httpContextLocatorService, navigationService)
        {
            _workspace = ZadsServices.Workspace;
        }
        public object CurrentObject
        {
            get
            {
                return GetCurrentContext().Session["CurrentObject"];
            }
            set
            {
                GetCurrentContext().Session["CurrentObject"] = value;
            }
        }
        #region User
        public IList<AppUser> GetProgramManagers()
        {
            return WorkspaceFactory.CreateReadOnly().Query<AppUser>(x => x.EmployeePosition.PositionName.Contains("Program Manager") || x.EmployeePosition.PositionName == "Vice President & Country Director, Ethiopia" || x.EmployeePosition.PositionName == "Finance Manager" || x.EmployeePosition.PositionName == "Senior Deputy Country Director" || x.EmployeePosition.PositionName== "Coordinator, Program Operations" || x.EmployeePosition.PositionName == "M&E Manager" || x.EmployeePosition.PositionName == "Head, Administration & HR").ToList();
        }
        public IList<AppUser> GetEmployeeList()
        {
            return WorkspaceFactory.CreateReadOnly().Query<AppUser>(x => x.IsActive == true).OrderBy(x=>x.FullName).ToList();
        }
        public IList<AppUser> GetAppUsersByEmployeePosition(int employeePosition)
        {
            return WorkspaceFactory.CreateReadOnly().Query<AppUser>(x => x.EmployeePosition.Id == employeePosition).ToList();
        }
        public AppUser GetUser(int userid)
        {
            return _workspace.Single<AppUser>(x => x.Id == userid, x => x.AppUserRoles.Select(y => y.Role));
        }
        public AppUser GetUserByUserName(string userName)
        {
            return _workspace.Single<AppUser>(x => x.UserName == userName, x => x.AppUserRoles.Select(y => y.Role));
        }
        #endregion
        #region Account
        public Account GetAccount(int AccountId)
        {
            return _workspace.Single<Account>(x => x.Id == AccountId);
        }
        public IList<Account> GetAccounts()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Account>(x=>x.Status =="Active").ToList();
        }
        public IList<Account> ListBankAccounts(string BankName)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM Accounts Where Status = 'Active' AND 1 = Case when '" + BankName + "' = '' Then 1 When Accounts.Name LIKE '%" + BankName + "%'  Then 1 END  ";

            return _workspace.SqlQuery<Account>(filterExpression).ToList();
        }
        #endregion
        #region ItemAccount
        public IList<ItemAccount> GetItemAccounts()
        {
            return WorkspaceFactory.CreateReadOnly().Query<ItemAccount>(x => x.Status == "Active").OrderBy(x => x.AccountName).ToList();
        }

       
        public ItemAccount GetItemAccount(int ItemAccountId)
        {
            return _workspace.Single<ItemAccount>(x => x.Id == ItemAccountId);
        }
        public IList<ItemAccount> ListItemAccounts(string ItemAccountName, string ItemAccountCode)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM ItemAccounts Where Status = 'Active' And 1 = Case when '" + ItemAccountName + "' = '' Then 1 When ItemAccounts.AccountName = '" + ItemAccountName + "'  Then 1 END AND 1 = Case when '" + ItemAccountCode + "' = '' Then 1 When ItemAccounts.AccountCode = '" + ItemAccountCode + "'  Then 1 END  ";

            return _workspace.SqlQuery<ItemAccount>(filterExpression).ToList();

        }
        public ItemAccount GetDefaultItemAccount()
        {
            return _workspace.Single<ItemAccount>(x => x.AccountCode == "13110");
        }
        #endregion
        #region Grant

        public IList<Grant> GetGrants()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Grant>(x => x.Status == "Active").ToList();
        }
        public Grant GetGrant(int id)
        {
            return _workspace.Single<Grant>(x => x.Id == id);
        }
        public Grant GetGrantByCode(string grantCode)
        {
            return _workspace.Single<Grant>(x => x.GrantCode == grantCode);
        }
        public IList<Grant> ListGrants(string GrantName, string GrantCode)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Grants Where Status = 'Active' AND 1 = Case when '" + GrantName + "' = '' Then 1 When Grants.GrantName = '" + GrantName + "'  Then 1 END AND 1 = Case when '" + GrantCode + "' = '' Then 1 When Grants.GrantCode = '" + GrantCode + "'  Then 1 END  ";

            return _workspace.SqlQuery<Grant>(filterExpression).ToList();

        }
        #endregion
        #region Supplier
        public IList<Supplier> GetSuppliers()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Supplier>(x => x.Status == "Active").OrderBy(x => x.SupplierName).ToList();
        }
        public IList<Supplier> GetSuppliers(int SupplierTypeId)
        {
            return WorkspaceFactory.CreateReadOnly().Query<Supplier>(x => x.SupplierType.Id == SupplierTypeId && x.Status =="Active").ToList();
        }
        public Supplier GetSupplier(int SupplierId)
        {
            return _workspace.Single<Supplier>(x => x.Id == SupplierId);
        }
        public IList<Supplier> ListSuppliers(string SupplierName)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Suppliers Where Status = 'Active' AND 1 = Case when '" + SupplierName + "' = '' Then 1 When Suppliers.SupplierName = '" + SupplierName + "'  Then 1 END  ";

            return _workspace.SqlQuery<Supplier>(filterExpression).ToList();

        }
        #endregion
        #region SoleVendorSupplier
        public IList<SoleVendorSupplier> GetSoleVendorSuppliers()
        {
            return WorkspaceFactory.CreateReadOnly().Query<SoleVendorSupplier>(x => x.Status == "Active").OrderBy(x => x.SupplierName).ToList();
        }
        public IList<SoleVendorSupplier> GetSoleVendorSuppliers(int SupplierTypeId)
        {
            return WorkspaceFactory.CreateReadOnly().Query<SoleVendorSupplier>(x => x.SupplierType.Id == SupplierTypeId && x.Status == "Active").ToList();
        }
        public SoleVendorSupplier GetSoleVendorSupplier(int SupplierId)
        {
            return _workspace.Single<SoleVendorSupplier>(x => x.Id == SupplierId);
        }
        public IList<SoleVendorSupplier> ListSoleVendorSuppliers(string SupplierName)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM SoleVendorSuppliers Where Status = 'Active' AND 1 = Case when '" + SupplierName + "' = '' Then 1 When SoleVendorSuppliers.SupplierName = '" + SupplierName + "'  Then 1 END  ";

            return _workspace.SqlQuery<SoleVendorSupplier>(filterExpression).ToList();

        }
        #endregion
        #region Supplier Type
        public IList<SupplierType> ListSupplierTypes(string email)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM SupplierTypes Where Status = 'Active' AND 1 = Case when '" + email + "' = '' Then 1 When SupplierTypes.SupplierTypename LIKE '%" + email + "%'  Then 1 END  ";

            return _workspace.SqlQuery<SupplierType>(filterExpression).ToList();
        }

        public SupplierType GetSupplierType(int id)
        {
            return _workspace.Single<SupplierType>(x => x.Id == id);
        }

        public IList<SupplierType> GetSupplierTypes()
        {
            return WorkspaceFactory.CreateReadOnly().Query<SupplierType>(x => x.Status == "Active").ToList();
        }
        #endregion
        #region CarRental
        public IList<CarRental> GetCarRentals()
        {
            return WorkspaceFactory.CreateReadOnly().Query<CarRental>(x => x.Status == "Active").ToList();
        }
        public CarRental GetCarRental(int CarRentalId)
        {
            return _workspace.Single<CarRental>(x => x.Id == CarRentalId);
        }
        public IList<CarRental> ListCarRentals(string CarRentalName)
        {
            string filterExpression = "";

            //filterExpression = "SELECT * FROM CarRentals Where Status = 'Active' AND 1 = Case when '" + CarRentalName + "' = '' Then 1 When CarRentals.Name LIKE '%" + CarRentalName + "%'  Then 1 END  ";
            filterExpression = "SELECT * FROM CarRentals Where 1 = Case when '" + CarRentalName + "' = '' Then 1 When CarRentals.Name LIKE '%" + CarRentalName + "%'  Then 1 END  ";

            return _workspace.SqlQuery<CarRental>(filterExpression).ToList();

        }
        #endregion
        #region CarModel
        public IList<CarModel> GetCarModels()
        {
            return WorkspaceFactory.CreateReadOnly().Query<CarModel>(null).OrderBy(x => x.ModelName).ToList();
        }
        public CarModel GetCarModel(int CarModelId)
        {
            return _workspace.Single<CarModel>(x => x.Id == CarModelId);
        }
        public IList<CarModel> ListCarModels(string CarModelName)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM CarModels Where  1 = Case when '" + CarModelName + "' = '' Then 1 When CarModels.ModelName LIKE '%" + CarModelName + "%'  Then 1 END  ";

            return _workspace.SqlQuery<CarModel>(filterExpression).ToList();

        }
        #endregion
        #region Vehicle
        public IList<Vehicle> GetVehicles()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Vehicle>(x => x.Status == "Active").ToList();
        }
        public Vehicle GetVehicle(int VehicleId)
        {
            return _workspace.Single<Vehicle>(x => x.Id == VehicleId);
        }
        public IList<Vehicle> ListVehicles(string PlateNo)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM Vehicles Where Status = 'Active' AND 1 = Case when '" + PlateNo + "' = '' Then 1 When Vehicles.PlateNo LIKE '%" + PlateNo + "%'  Then 1 END  ";

            return _workspace.SqlQuery<Vehicle>(filterExpression).ToList();

        }
        #endregion
        #region LeaveType

        public IList<LeaveType> GetLeaveTypes()
        {
            return WorkspaceFactory.CreateReadOnly().Query<LeaveType>(x => x.Status == "Active").ToList();
        }
        public LeaveType GetLeaveType(int LeaveTypeId)
        {
            return _workspace.Single<LeaveType>(x => x.Id == LeaveTypeId);
        }
        public IList<LeaveType> ListLeaveTypes()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM LeaveTypes Where Status = 'Active' ";

            return _workspace.SqlQuery<LeaveType>(filterExpression).ToList();

        }
        #endregion
        #region Holiday

        public Holiday GetHoliday(int HolidayId)
        {
            return _workspace.Single<Holiday>(x => x.Id == HolidayId);
        }
        public IList<Holiday> ListHolidays()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Holidays ";

            return _workspace.SqlQuery<Holiday>(filterExpression).ToList();

        }
        #endregion
        #region EmployeePosition

        public IList<EmployeePosition> GetEmployeePositions()
        {
            return WorkspaceFactory.CreateReadOnly().Query<EmployeePosition>(x => x.Status == "Active").ToList();
        }
        public EmployeePosition GetEmployeePosition(int EmployeePositionId)
        {
            return _workspace.Single<EmployeePosition>(x => x.Id == EmployeePositionId);
        }
        public IList<EmployeePosition> ListEmployeePositions()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM EmployeePositions Where Status = 'Active'";

            return _workspace.SqlQuery<EmployeePosition>(filterExpression).ToList();

        }
        #endregion
        #region JobTitle

        public IList<JobTitle> GetJobTitles()
        {
            return WorkspaceFactory.CreateReadOnly().Query<JobTitle>(null).OrderBy(x => x.JobTitleName).ToList();
        }
        public JobTitle GetJobTitle(int JTId)
        {
            return _workspace.Single<JobTitle>(x => x.Id == JTId);
        }
        public IList<JobTitle> ListJobTitles()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM JobTitles ";

            return _workspace.SqlQuery<JobTitle>(filterExpression).ToList();

        }
        #endregion
        #region Program

        public IList<Program> GetPrograms()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Program>(null).OrderBy(x => x.ProgramName).ToList();
        }
        public Program GetProgram(int progId)
        {
            return _workspace.Single<Program>(x => x.Id == progId);
        }
        public IList<Program> ListPrograms()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Programs ";

            return _workspace.SqlQuery<Program>(filterExpression).ToList();

        }
        #endregion
        #region Project

        public IList<Project> GetProjects()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Project>(x=>x.Status == "Active").ToList();
        }
        public Project GetProject(int ProjectId)
        {
            return _workspace.Single<Project>(x => x.Id == ProjectId, x => x.ProGrants,x=>x.AppUser, x => x.ProGrants.Select(y => y.Grant));
        }
        public Project GetProjectforCostSharing(int ProjectId)
        {
            return _workspace.Single<Project>(x => x.Id == ProjectId && x.Status=="Active", x => x.ProGrants, x => x.AppUser, x => x.ProGrants.Select(y => y.Grant));
        }
        public ProGrant GetProjectGrant(int ProjectGrantId)
        {
            return _workspace.Single<ProGrant>(x => x.Id == ProjectGrantId);
        }
        public IList<ProGrant> GetProjectGrants()
        {
            return WorkspaceFactory.CreateReadOnly().Query<ProGrant>(null).ToList();
        }
        public IList<Grant> GetProjectGrantsByprojectId(int projectId)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Grants Left Join ProGrants on ProGrants.Grant_Id = Grants.Id Left Join Projects on Projects.Id = ProGrants.Project_Id  Where Projects.Id = '" + projectId + "' ";

            return _workspace.SqlQuery<Grant>(filterExpression).ToList();

        }        
        public IList<Project> ListProjects(string ProjectCode)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM Projects Where Projects.Status = 'Active' AND 1 = Case when '" + ProjectCode + "' = '' Then 1 When Projects.ProjectCode = '" + ProjectCode + "'  Then 1 END";

            return _workspace.SqlQuery<Project>(filterExpression).ToList();

        }
        #endregion
        #region CostSharingsetting

        public IList<CostSharingSetting> GetCostSharingSettings()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM CostSharingSettings Left join Projects on CostSharingSettings.ProjectId=Projects.Id";

            return _workspace.SqlQuery<CostSharingSetting>(filterExpression).ToList();
            
        }
        public CostSharingSetting GetProjectfromCostSharingSettings(int projectId)
        {
            return _workspace.Single<CostSharingSetting>(x => x.Project.Id == projectId);
        }
        #endregion
        #region ApprovalSetting

        public IList<ApprovalSetting> GetApprovalSettings()
        {
            return WorkspaceFactory.CreateReadOnly().Query<ApprovalSetting>(null).ToList();
        }
        public ApprovalSetting GetApprovalSetting(int ApprovalSettingId)
        {
            return _workspace.Single<ApprovalSetting>(x => x.Id == ApprovalSettingId);
        }
        public ApprovalLevel GetApprovalLevel(int ApprovalLevelId)
        {
            return _workspace.Single<ApprovalLevel>(x => x.Id == ApprovalLevelId);
        }
        public IList<ApprovalSetting> ListApprovalSettings(string Requesttype)
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM ApprovalSettings Where 1 = Case when '" + Requesttype + "' = '' Then 1 When ApprovalSettings.RequestType = '" + Requesttype + "'  Then 1 END";

            return _workspace.SqlQuery<ApprovalSetting>(filterExpression).ToList();

        }
        public ApprovalSetting GetApprovalSettingforProcess(string Requesttype, decimal value)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM ApprovalSettings Where ApprovalSettings.RequestType = '" + Requesttype + "'";

            IList<ApprovalSetting> settinglist = _workspace.SqlQuery<ApprovalSetting>(filterExpression).ToList();

            foreach (ApprovalSetting s in settinglist)
            {
                if (value < s.Value && "<" == s.CriteriaCondition)
                {
                    return s;
                }
                else if (value >= s.Value && value <= s.Value2 && "Between" == s.CriteriaCondition)
                {
                    return s;
                }
                else if (value >= s.Value && ">" == s.CriteriaCondition)
                {
                    return s;
                }
                else if (value == 0 && "None" == s.CriteriaCondition)
                {
                    return s;
                }

            }
            return null;
        }
        public ApprovalSetting GetApprovalSettingforPurchaseProcess(string Requesttype, decimal value)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM ApprovalSettings Where ApprovalSettings.RequestType = '" + Requesttype + "'";

            IList<ApprovalSetting> settinglist = _workspace.SqlQuery<ApprovalSetting>(filterExpression).ToList();

            foreach (ApprovalSetting s in settinglist)
            {
                
                    if (value < s.Value && "<" == s.CriteriaCondition)
                    {
                        return s;
                    }
                    else if (value >= s.Value && value <= s.Value2 && "Between" == s.CriteriaCondition)
                    {
                        return s;
                    }
                    else if (value >= s.Value && ">" == s.CriteriaCondition)
                    {
                        return s;
                    }
                    else if (value == 0 && "None" == s.CriteriaCondition)
                    {
                        return s;
                    }
                }
                
                  return null;
            
           
        }
        #endregion
        #region EmployeeSetting
        public IList<EmployeeLeave> GetEmployeeLeaves()
        {
            return WorkspaceFactory.CreateReadOnly().Query<EmployeeLeave>(null).ToList();
        }
        public EmployeeLeave GetEmployeeLeave(int Id)
        {
            return _workspace.Single<EmployeeLeave>(x => x.Id == Id);
        }
        public EmployeeLeave GetActiveEmployeeLeaveRequest(int UserId,bool Status)
        {
            //return WorkspaceFactory.CreateReadOnly().Query<EmployeeLeave>(x => x.AppUser.Id == UserId && x.Status == Status).SingleOrDefault();
            return _workspace.Single<EmployeeLeave>(x => x.AppUser.Id == UserId && x.Status == Status);
               // .SingleOrDefault();
        }
        public EmployeeLeave GetActiveEmployeeLeave(int UserId, bool Status)
        {
            return _workspace.Single<EmployeeLeave>(x => x.AppUser.Id == UserId && x.Status == Status);
        }
        public IList<EmployeeLeave> GetEmployeeLeaves(int UserId)
        {
            return  WorkspaceFactory.CreateReadOnly().Query<EmployeeLeave>(x => x.AppUser.Id == UserId).OrderByDescending(x=>x.Id).ToList();
        }
      

        #endregion
        #region Beneficiary
        public IList<Beneficiary> GetBeneficiaries()
        {
            return WorkspaceFactory.CreateReadOnly().Query<Beneficiary>(x => x.Status == "Active").ToList();
        }
        public Beneficiary GetBeneficiary(int BeneficiaryId)
        {
            return _workspace.Single<Beneficiary>(x => x.Id == BeneficiaryId);
        }
        public IList<Beneficiary> ListBeneficiaries(string BeneficiaryName)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM Beneficiaries Where Status = 'Active' AND 1 = Case when '" + BeneficiaryName + "' = '' Then 1 When Beneficiaries.BeneficiaryName LIKE '%" + BeneficiaryName + "%'  Then 1 END  ";

            return _workspace.SqlQuery<Beneficiary>(filterExpression).ToList();

        }
        #endregion
        #region ExpenseType

        public IList<ExpenseType> GetExpenseTypes()
        {
            return WorkspaceFactory.CreateReadOnly().Query<ExpenseType>(x => x.Status == "Active").ToList();
        }
        public ExpenseType GetExpenseType(int ExpenseTypeId)
        {
            return _workspace.Single<ExpenseType>(x => x.Id == ExpenseTypeId);
        }
        public IList<ExpenseType> ListExpenseTypes()
        {
            string filterExpression = "";

            filterExpression = "SELECT  *  FROM ExpenseTypes Where Status = 'Active'";

            return _workspace.SqlQuery<ExpenseType>(filterExpression).ToList();

        }
        #endregion
        #region Employee Setting
        public IList<Employee> GetEmployees(string FullName)
        {
            string filterExpression = "";

            filterExpression = "SELECT * FROM Employees Where 1 = Case when '" + FullName + "' = '' Then 1 When (Employees.FirstName + ' ' + Employees.lastName) like '%" + FullName + "%' Then 1 END order by FirstName";
            // return WorkspaceFactory.CreateReadOnly().Queryable<CashPaymentRequest>(filterExpression).ToList();
            return _workspace.SqlQuery<Employee>(filterExpression).ToList();
        }
        public Employee GetEmployee(int empId)
        {
            return _workspace.Single<Employee>(x => x.Id == empId);
        }
        public decimal TotalleaveTaken(int EmpId, DateTime Leavedatesetting)
        {
             string filterExpression = "";

            filterExpression = "SELECT *  FROM LeaveRequests Inner Join LeaveTypes on LeaveRequests.LeaveType_Id = LeaveTypes.Id "
                               + " Where LeaveTypes.LeaveTypeName = 'Annual Leave' and LeaveRequests.CurrentStatus= 'Issued' and LeaveRequests.Requester = '" + EmpId + "' and LeaveRequests.RequestedDate >= '" + Leavedatesetting+"'";
            // return WorkspaceFactory.CreateReadOnly().Queryable<CashPaymentRequest>(filterExpression).ToList();
           IList<LeaveRequest> EmpLeaverequest =  _workspace.SqlQuery<LeaveRequest>(filterExpression).ToList();
           return EmpLeaverequest.Sum(x => x.RequestedDays);
                            


        }
        #endregion
        #region Entity Manipulation
        public void SaveOrUpdateEntity<T>(T item) where T : class
        {
            IEntity entity = (IEntity)item;
            if (entity.Id == 0)
                _workspace.Add<T>(item);
            else
                _workspace.Update<T>(item);

            _workspace.CommitChanges();
            _workspace.Refresh(item);
        }
        public void DeleteEntity<T>(T item) where T : class
        {
            _workspace.Delete<T>(item);
            _workspace.CommitChanges();
            _workspace.Refresh(item);
        }

        public void Commit()
        {
            _workspace.CommitChanges();
        }
        #endregion           
    }
}
