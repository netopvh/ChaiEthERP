﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Chai.WorkflowManagment.CoreDomain.Requests;
using Chai.WorkflowManagment.CoreDomain.Setting;
using Chai.WorkflowManagment.CoreDomain.Users;
using Chai.WorkflowManagment.Enums;
using Chai.WorkflowManagment.Modules.Approval.Views;
using Chai.WorkflowManagment.Shared;
using Chai.WorkflowManagment.Shared.MailSender;
using log4net;
using log4net.Config;
using Microsoft.Practices.ObjectBuilder;

namespace Chai.WorkflowManagment.Modules.Approval.Views
{
    public partial class frmVehicleApproval : POCBasePage, IVehicleApprovalView
    {
        private VehicleApprovalPresenter _presenter;
        private static readonly ILog Log = LogManager.GetLogger("AuditTrailLog");
        private bool needsApproval = false;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this._presenter.OnViewInitialized();
                XmlConfigurator.Configure();
                PopProgressStatus();
                BindVehicles();
                PopProjects();

            }
            this._presenter.OnViewLoaded();
            BindSearchVehicleRequestGrid();
            if (_presenter.CurrentVehicleRequest.Id != 0)
                PrintTransaction();
        }
        [CreateNew]
        public VehicleApprovalPresenter Presenter
        {
            get
            {
                return this._presenter;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                this._presenter = value;
                this._presenter.View = this;
            }
        }
        public override string PageID
        {
            get
            {
                return "{7E42140E-DD62-4230-983E-32BD9FA35817}";
            }
        }
        #region Field Getters
        public int GetVehicleRequestId
        {
            get
            {
                if (grvVehicleRequestList.SelectedDataKey != null)
                {
                    return Convert.ToInt32(grvVehicleRequestList.SelectedDataKey.Value);
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion
        private void PopProgressStatus()
        {
            string[] s = Enum.GetNames(typeof(ProgressStatus));

            for (int i = 0; i < s.Length; i++)
            {
                ddlSrchProgressStatus.Items.Add(new ListItem(s[i].Replace('_', ' '), s[i].Replace('_', ' ')));
                ddlSrchProgressStatus.DataBind();
            }
        }
        private void PopApprovalStatus()
        {
            ddlApprovalStatus.Items.Clear();
            ddlApprovalStatus.Items.Add(new ListItem("Select Status", "0"));
            string[] s = Enum.GetNames(typeof(ApprovalStatus));

            for (int i = 0; i < s.Length; i++)
            {
                if (GetWillStatus().Substring(0, 2) == s[i].Substring(0, 2))
                {
                    ddlApprovalStatus.Items.Add(new ListItem(s[i].Replace('_', ' '), s[i].Replace('_', ' ')));
                }

            }
            ddlApprovalStatus.Items.Add(new ListItem(ApprovalStatus.Rejected.ToString().Replace('_', ' '), ApprovalStatus.Rejected.ToString().Replace('_', ' ')));
            if (_presenter.CurrentVehicleRequest.ProgressStatus == ProgressStatus.Completed.ToString())
            {
                ddlApprovalStatus.Items.Add(new ListItem(ApprovalStatus.Canceled.ToString().Replace('_', ' '), ApprovalStatus.Canceled.ToString().Replace('_', ' ')));
            }

        }
        private string GetWillStatus()
        {
            ApprovalSetting AS = _presenter.GetApprovalSettingforProcess(RequestType.Vehicle_Request.ToString().Replace('_', ' ').ToString(), 0);
            string will = "";
            foreach (ApprovalLevel AL in AS.ApprovalLevels)
            {
                if ((AL.EmployeePosition.PositionName == "Superviser/Line Manager" || AL.EmployeePosition.PositionName == "Program Manager") && _presenter.CurrentVehicleRequest.CurrentLevel == 1)
                {
                    will = "Approve";
                    break;

                }
                else if (_presenter.GetUser(_presenter.CurrentVehicleRequest.CurrentApprover).EmployeePosition.PositionName == AL.EmployeePosition.PositionName)
                {
                    will = AL.Will;
                }

            }
            return will;
        }
        private void PopDrivers(DropDownList ddlDriver)
        {
            ddlDriver.DataSource = _presenter.GetDrivers();
            ddlDriver.DataTextField = "FullName";
            ddlDriver.DataValueField = "ID";
            ddlDriver.DataBind();
        }
        private void PopCarRentals(DropDownList ddlCarRental)
        {
            ddlCarRental.DataSource = _presenter.GetCarRentals();
            ddlCarRental.DataTextField = "Name";
            ddlCarRental.DataValueField = "ID";
            ddlCarRental.DataBind();
        }
        private void PopCarModels(DropDownList ddlCarModel)
        {
            ddlCarModel.DataSource = _presenter.GetCarModels();
            ddlCarModel.DataTextField = "ModelName";
            ddlCarModel.DataValueField = "ID";
            ddlCarModel.DataBind();
        }
        private void PopVehicles(DropDownList ddlVehicles)
        {
            ddlVehicles.DataSource = _presenter.GetVehicles();
            ddlVehicles.DataTextField = "PlateNo";
            ddlVehicles.DataValueField = "ID";
            ddlVehicles.DataBind();
        }
        private void PopProjects()
        {
            ddlProject.DataSource = _presenter.GetProjects();
            ddlProject.DataBind();

            ddlProject.Items.Insert(0, new ListItem("---Select Project---", "0"));
            ddlProject.SelectedIndex = 0;
        }
        private void PopGrants(int ProjectId)
        {
            ddlGrant.DataSource = _presenter.GetGrantbyprojectId(ProjectId);
            ddlGrant.DataBind();

            ddlGrant.Items.Insert(0, new ListItem("---Select Grant---", "0"));
            ddlGrant.SelectedIndex = 0;
        }
        private void BindSearchVehicleRequestGrid()
        {
            grvVehicleRequestList.DataSource = _presenter.ListVehicleRequests(txtSrchRequestNo.Text, txtSrchRequestDate.Text, ddlSrchProgressStatus.SelectedValue);
            grvVehicleRequestList.DataBind();
        }
        private void BindVehicleRequestStatus()
        {
            // VehicleApprovalPresenter _presenterm = new   VehicleApprovalPresenter;
            foreach (VehicleRequestStatus VRS in _presenter.CurrentVehicleRequest.VehicleRequestStatuses)
            {
                if (VRS.WorkflowLevel == _presenter.CurrentVehicleRequest.CurrentLevel && _presenter.CurrentVehicleRequest.ProgressStatus != ProgressStatus.Completed.ToString())
                {
                    btnApprove.Enabled = true;
                }
                if (_presenter.CurrentVehicleRequest.CurrentLevel == _presenter.CurrentVehicleRequest.VehicleRequestStatuses.Count && !String.IsNullOrEmpty(VRS.ApprovalStatus))
                {
                    btnPrint.Enabled = true;
                }
            }
        }
        private void BindVehicles()
        {
            dgVehicles.DataSource = _presenter.CurrentVehicleRequest.VehicleRequestDetails;
            dgVehicles.DataBind();

        }
        private void ShowPrint()
        {
            if (_presenter.CurrentVehicleRequest.ProgressStatus == ProgressStatus.Completed.ToString())
            {
                if (_presenter.CurrentVehicleRequest.CurrentStatus != "Rejected")
                {
                    btnPrint.Enabled = true;
                    btnPrintTravellog.Visible = true;
                                     
                }
                SendEmailToRequester();
            }
        }
        private void SendEmail(VehicleRequestStatus VRS)
        {
            if (_presenter.GetUser(VRS.Approver).IsAssignedJob != true)
            {
                EmailSender.Send(_presenter.GetUser(VRS.Approver).Email, "Vehicle Request", (_presenter.CurrentVehicleRequest.AppUser.FullName).ToUpper() + " Requests  Vehicle for Request No. " + (_presenter.CurrentVehicleRequest.RequestNo).ToUpper());
            }
            else
            {
                EmailSender.Send(_presenter.GetUser(_presenter.GetAssignedJobbycurrentuser(VRS.Approver).AssignedTo).Email, "Vehicle Request", (_presenter.CurrentVehicleRequest.AppUser.FullName).ToUpper() + "Requests Vehicle for Request No." + (_presenter.CurrentVehicleRequest.RequestNo).ToUpper());
            }
        }
        private void SendEmailRejected(VehicleRequestStatus VRS)
        {
            EmailSender.Send(_presenter.GetUser(_presenter.CurrentVehicleRequest.AppUser.Id).Email, "Vehicle Request Rejection", " Your Vehicle Request with RequestNo." + (_presenter.CurrentVehicleRequest.RequestNo).ToUpper() + " made by " + (_presenter.CurrentVehicleRequest.AppUser.FullName).ToUpper() + " was Rejected by " + _presenter.CurrentUser().FullName + " for reason" + (VRS.RejectedReason).ToUpper());
            Log.Info(_presenter.GetUser(VRS.Approver).FullName + " has rejected a Vehicle Request made by " + _presenter.CurrentVehicleRequest.AppUser.FullName);
        }
        private void SendCompletedEmail(VehicleRequestStatus VRS)
        {
            foreach (VehicleRequestDetail assignedVehicle in _presenter.CurrentVehicleRequest.VehicleRequestDetails)
            {
                EmailSender.Send(_presenter.GetUser(_presenter.CurrentVehicleRequest.AppUser.Id).Email, "Vehicle Request ", "Your Vehicle Request has been proccessed by " + (_presenter.GetUser(VRS.Approver).FullName).ToUpper() + " and Your assigned Driver is " + (assignedVehicle.AppUser.FullName).ToUpper() + ". The Car's Plate Number is " + (assignedVehicle.PlateNo).ToUpper());

                Log.Info(_presenter.GetUser(VRS.Approver).FullName + " has approved a Vehicle Request made by " + (_presenter.CurrentVehicleRequest.AppUser.FullName).ToUpper() + " and assigned a Car Rental company named " + (_presenter.GetCarRental(assignedVehicle.CarRental.Id).Name).ToUpper());

            }
        }
        private void SendEmailToRequester()
        {
            foreach (VehicleRequestDetail assignedVehicle in _presenter.CurrentVehicleRequest.VehicleRequestDetails)
            {
                string assignedDriver = "";
                if (assignedVehicle.AppUser != null)
                    assignedDriver = (assignedVehicle.AppUser.FullName).ToUpper();
                else
                    assignedDriver = assignedVehicle.RentalDriverName;
                EmailSender.Send(_presenter.GetUser(_presenter.CurrentVehicleRequest.AppUser.Id).Email, "Vehicle Request Completion", " Your Vehicle Request was Completed. And your assigned driver is " + assignedDriver + ". The driver's phone number is " + assignedVehicle.DriverPhoneNo);
            }
        }
        private void SendCanceledEmail()
        {
            VehicleRequest thisRequest = _presenter.CurrentVehicleRequest;
            //To the requester
            EmailSender.Send(_presenter.GetUser(thisRequest.AppUser.Id).Email, "Vehicle Request Canceled", " Your Vehicle Request with Request No. " + thisRequest.RequestNo + " was Canceled!");
            //To the approvers
            foreach (VehicleRequestStatus statuses in thisRequest.VehicleRequestStatuses)
            {
                EmailSender.Send(_presenter.GetUser(statuses.Approver).Email, "Vehicle Request Canceled", " The Vehicle Request with Request No. " + thisRequest.RequestNo + " has been Canceled!");
            }
        }
        private void SendEmailDriver(VehicleRequestStatus VRS)
        {
            foreach (VehicleRequestDetail assignedVehicle in _presenter.CurrentVehicleRequest.VehicleRequestDetails)
            {
                if (assignedVehicle.AppUser != null)
                {
                    EmailSender.Send(_presenter.GetUser(assignedVehicle.AppUser.Id).Email, "Vehicle Request ", "You are assigned to give a drive to " + (_presenter.CurrentVehicleRequest.AppUser.FullName).ToUpper() + " and your assigned Car Plate Number is " + (assignedVehicle.PlateNo).ToUpper());
                    Log.Info(_presenter.GetUser(VRS.Approver).FullName + " has approved a Vehicle Request made by " + _presenter.CurrentVehicleRequest.AppUser.FullName);
                }
            }
        }
        private void GetNextApprover()
        {
            foreach (VehicleRequestStatus VRS in _presenter.CurrentVehicleRequest.VehicleRequestStatuses)
            {
                if (VRS.ApprovalStatus == null)
                {
                    SendEmail(VRS);
                    _presenter.CurrentVehicleRequest.CurrentApprover = VRS.Approver;
                    _presenter.CurrentVehicleRequest.CurrentLevel = VRS.WorkflowLevel;
                    _presenter.CurrentVehicleRequest.ProgressStatus = ProgressStatus.InProgress.ToString();
                    break;

                }
            }
        }
        private void SaveVehicleRequestStatus()
        {
            foreach (VehicleRequestStatus VRS in _presenter.CurrentVehicleRequest.VehicleRequestStatuses)
            {
                if ((VRS.Approver == _presenter.CurrentUser().Id || _presenter.CurrentUser().Id == (_presenter.GetAssignedJobbycurrentuser(VRS.Approver) != null ? _presenter.GetAssignedJobbycurrentuser(VRS.Approver).AssignedTo : 0)) && VRS.WorkflowLevel == _presenter.CurrentVehicleRequest.CurrentLevel)
                {
                    VRS.Date = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                    VRS.ApprovalStatus = ddlApprovalStatus.SelectedValue;
                    VRS.RejectedReason = txtRejectedReason.Text;
                    VRS.AssignedBy = _presenter.GetAssignedJobbycurrentuser(VRS.Approver) != null ? _presenter.GetAssignedJobbycurrentuser(VRS.Approver).AppUser.FullName : "";
                    VRS.Comment = txtComment.Text;
                    if (VRS.ApprovalStatus != ApprovalStatus.Rejected.ToString())
                    {
                        if (_presenter.CurrentVehicleRequest.CurrentLevel == _presenter.CurrentVehicleRequest.VehicleRequestStatuses.Count)
                        {
                            _presenter.CurrentVehicleRequest.ProgressStatus = ProgressStatus.Completed.ToString();
                            _presenter.CurrentVehicleRequest.CurrentStatus = VRS.ApprovalStatus;
                            VRS.Approver = _presenter.CurrentUser().Id;
                            _presenter.CurrentVehicleRequest.CurrentLevel = VRS.WorkflowLevel;
                            SendEmailDriver(VRS);
                            //  SendCompletedEmail(VRS);
                            break;
                        }
                        else
                        {
                            GetNextApprover();
                        }
                        // _presenter.CurrentVehicleRequest.CurrentStatus = VRS.ApprovalStatus;
                        //GetNextApprover();
                    }
                    else if(VRS.ApprovalStatus == ApprovalStatus.Canceled.ToString())
                     {
                        _presenter.CurrentVehicleRequest.ProgressStatus = ProgressStatus.Completed.ToString();
                        _presenter.CurrentVehicleRequest.CurrentStatus = VRS.ApprovalStatus;
                        VRS.Approver = _presenter.CurrentUser().Id;
                        SendCanceledEmail();
                    }
                    else
                    {
                        _presenter.CurrentVehicleRequest.ProgressStatus = ProgressStatus.Completed.ToString();
                        _presenter.CurrentVehicleRequest.CurrentStatus = VRS.ApprovalStatus;
                        VRS.Approver = _presenter.CurrentUser().Id;
                        SendEmailRejected(VRS);
                    }
                    break;
                }
            }
        }
        protected void grvVehicleRequestList_SelectedIndexChanged(object sender, EventArgs e)
        {
            //grvVehicleRequestList.SelectedDataKey.Value            
            _presenter.OnViewLoaded();
            PopApprovalStatus();
            BindVehicleRequestStatus();
            BindVehicles();
            ddlProject.SelectedValue = _presenter.CurrentVehicleRequest.Project.Id.ToString();
            ddlProject_SelectedIndexChanged(sender, e);
            if (_presenter.CurrentVehicleRequest.Grant != null)
                ddlGrant.SelectedValue = _presenter.CurrentVehicleRequest.Grant.Id.ToString();
            if (_presenter.CurrentVehicleRequest.ProgressStatus == ProgressStatus.Completed.ToString())
            {
                btnApprove.Enabled = false;
                PrintTransaction();
            }
            txtRejectedReason.Visible = false;
            rfvRejectedReason.Enabled = false;
            pnlApproval_ModalPopupExtender.Show();
            if (_presenter.CurrentVehicleRequest.CurrentLevel == _presenter.CurrentVehicleRequest.VehicleRequestStatuses.Count)
                pnlApproverview.Visible = true;
            if (_presenter.CurrentVehicleRequest.ProgressStatus == ProgressStatus.Completed.ToString())
            {

                btnPrintTravellog.Visible = true;
            }


        }
        protected void grvVehicleRequestList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            Button btnStatus = e.Row.FindControl("btnStatus") as Button;
            VehicleRequest CSR = e.Row.DataItem as VehicleRequest;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (CSR.ProgressStatus == ProgressStatus.InProgress.ToString())
                {
                    btnStatus.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFFF6C");

                }
                else if (CSR.ProgressStatus == ProgressStatus.Completed.ToString())
                {
                    btnStatus.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF7251");

                }
            }
        }
        protected void grvVehicleRequestList_RowCommand(Object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Page")
            {
                // If multiple ButtonField column fields are used, use the
                // CommandName property to determine which button was clicked.
                if (e.CommandName == "TravelLog")
                {
                    // Convert the row index stored in the CommandArgument
                    // property to an Integer.
                    int index = Convert.ToInt32(e.CommandArgument);

                    int rowID = Convert.ToInt32(grvVehicleRequestList.DataKeys[index].Value);
                    string url = String.Format("~/Request/frmTravelLog.aspx?requestId={0}", rowID);
                    _presenter.navigate(url);
                }
            }

        }
        protected void grvVehicleRequestList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            grvVehicleRequestList.PageIndex = e.NewPageIndex;
            btnFind_Click(sender, e);
        }
        protected void dgVehicles_CancelCommand(object source, DataGridCommandEventArgs e)
        {
            this.dgVehicles.EditItemIndex = -1;
        }
        protected void dgVehicles_DeleteCommand(object source, DataGridCommandEventArgs e)
        {
            int id = (int)dgVehicles.DataKeys[e.Item.ItemIndex];
            int vehicleId = (int)dgVehicles.DataKeys[e.Item.ItemIndex];
            VehicleRequestDetail vehicle;

            if (vehicleId > 0)
                vehicle = _presenter.CurrentVehicleRequest.GetVehicle(vehicleId);
            else
                vehicle = (VehicleRequestDetail)_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.ItemIndex];
            try
            {
                if (vehicleId > 0)
                {
                    _presenter.CurrentVehicleRequest.RemoveVehicle(id);
                    if (_presenter.GetVehicleById(id) != null)
                        _presenter.DeleteVehicles(_presenter.GetVehicleById(id));
                    _presenter.SaveOrUpdateVehicleRequest(_presenter.CurrentVehicleRequest);
                }
                else { _presenter.CurrentVehicleRequest.VehicleRequestDetails.Remove(vehicle); }
                BindVehicles();
                ddlProject.SelectedValue = _presenter.CurrentVehicleRequest.Project.Id.ToString();
                if (_presenter.CurrentVehicleRequest.Grant != null)
                    ddlGrant.SelectedValue = _presenter.CurrentVehicleRequest.Grant.Id.ToString();
                pnlApproval_ModalPopupExtender.Show();

                Master.ShowMessage(new AppMessage("Vehicle Information was Removed Successfully", Chai.WorkflowManagment.Enums.RMessageType.Info));
            }
            catch (Exception ex)
            {
                Master.ShowMessage(new AppMessage("Error: Unable to delete Vehicle Information. " + ex.Message, Chai.WorkflowManagment.Enums.RMessageType.Error));
            }
        }
        protected void dgVehicles_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            VehicleRequestDetail Vehicle = new VehicleRequestDetail();
            if (e.CommandName == "AddNew")
            {
                try
                {
                    DropDownList ddlAssignedVehicle = e.Item.FindControl("ddlAssignedVehicle") as DropDownList;
                    Vehicle.AssignedVehicle = ddlAssignedVehicle.SelectedValue;
                    DropDownList ddlFPlateNo = e.Item.FindControl("ddlFPlateNo") as DropDownList;
                    Vehicle.PlateNo = ddlFPlateNo.SelectedItem.Text;
                    TextBox txtRate = e.Item.FindControl("txtFRate") as TextBox;
                    Vehicle.Rate = Convert.ToDecimal(txtRate.Text);
                    DropDownList ddlCarRental = e.Item.FindControl("ddlCarRental") as DropDownList;
                    Vehicle.CarRental = _presenter.GetCarRental(Convert.ToInt32(ddlCarRental.SelectedValue));
                    TextBox txtFDriverPhoneNo = e.Item.FindControl("txtFDriverPhoneNo") as TextBox;
                    Vehicle.DriverPhoneNo = txtFDriverPhoneNo.Text;
                    TextBox txtFRentalDrName = e.Item.FindControl("txtFRentalDrName") as TextBox;
                    Vehicle.RentalDriverName = txtFRentalDrName.Text;
                    DropDownList ddlCarModel = e.Item.FindControl("ddlFCarModel") as DropDownList;
                    Vehicle.CarModel = _presenter.GetCarModel(Convert.ToInt32(ddlCarModel.SelectedValue));
                    DropDownList ddlDriver = e.Item.FindControl("ddlDriver") as DropDownList;
                    Vehicle.AppUser = _presenter.GetUser(Convert.ToInt32(ddlDriver.SelectedValue));

                    _presenter.CurrentVehicleRequest.VehicleRequestDetails.Add(Vehicle);

                    dgVehicles.EditItemIndex = -1;
                    BindVehicles();
                    ddlProject.SelectedValue = _presenter.CurrentVehicleRequest.Project.Id.ToString();
                    if (_presenter.CurrentVehicleRequest.Grant != null)
                        ddlGrant.SelectedValue = _presenter.CurrentVehicleRequest.Grant.Id.ToString();
                    pnlApproval_ModalPopupExtender.Show();
                    Master.ShowMessage(new AppMessage("Vehicle Information Successfully Added", RMessageType.Info));
                }
                catch (Exception ex)
                {
                    Master.ShowMessage(new AppMessage("Error: Unable to Add Vehicle Information " + ex.Message, RMessageType.Error));
                }
            }
        }
        protected void dgVehicles_EditCommand(object source, DataGridCommandEventArgs e)
        {
            this.dgVehicles.EditItemIndex = e.Item.ItemIndex;
            BindVehicles();
            ddlProject.SelectedValue = _presenter.CurrentVehicleRequest.Project.Id.ToString();
            if (_presenter.CurrentVehicleRequest.Grant != null)
                ddlGrant.SelectedValue = _presenter.CurrentVehicleRequest.Grant.Id.ToString();
            pnlApproval_ModalPopupExtender.Show();
        }
        protected void dgVehicles_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Footer)
            {
                DropDownList ddlCarRental = e.Item.FindControl("ddlCarRental") as DropDownList;
                PopCarRentals(ddlCarRental);
                DropDownList ddlCarModel = e.Item.FindControl("ddlFCarModel") as DropDownList;
                PopCarModels(ddlCarModel);
                DropDownList ddlDriver = e.Item.FindControl("ddlDriver") as DropDownList;
                PopDrivers(ddlDriver);
                DropDownList ddlVehicle = e.Item.FindControl("ddlFPlateNo") as DropDownList;
                PopVehicles(ddlVehicle);


            }
            else
            {
                if (_presenter.CurrentVehicleRequest.VehicleRequestDetails != null)
                {
                    DropDownList ddlEdtAssignedVehicle = e.Item.FindControl("ddlEdtAssignedVehicle") as DropDownList;
                    if (ddlEdtAssignedVehicle != null)
                    {
                        if (_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].AssignedVehicle != "")
                        {
                            ListItem liI = ddlEdtAssignedVehicle.Items.FindByValue(_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].AssignedVehicle);
                            if (liI != null)
                                liI.Selected = true;
                        }
                    }
                    DropDownList ddlEdtCarRental = e.Item.FindControl("ddlEdtCarRental") as DropDownList;
                    if (ddlEdtCarRental != null)
                    {
                        PopCarRentals(ddlEdtCarRental);
                        if (_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].CarRental != null)
                        {
                            ListItem liI = ddlEdtCarRental.Items.FindByValue(_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].CarRental.Id.ToString());
                            if (liI != null)
                                liI.Selected = true;
                        }
                    }
                    DropDownList ddlEdtCarModel = e.Item.FindControl("ddlEdtCarModel") as DropDownList;
                    if (ddlEdtCarModel != null)
                    {
                        PopCarModels(ddlEdtCarModel);
                        if (_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].CarModel != null)
                        {
                            ListItem liI = ddlEdtCarModel.Items.FindByValue(_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].CarModel.Id.ToString());
                            if (liI != null)
                                liI.Selected = true;
                        }
                    }
                    DropDownList ddlEdtDriver = e.Item.FindControl("ddlEdtDriver") as DropDownList;
                    if (ddlEdtDriver != null)
                    {
                        PopDrivers(ddlEdtDriver);
                        if (_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].AppUser != null)
                        {
                            ListItem liI = ddlEdtDriver.Items.FindByValue(_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].AppUser.Id.ToString());
                            if (liI != null)
                                liI.Selected = true;
                        }
                    }
                    DropDownList ddlEdtPlateNo = e.Item.FindControl("ddlEdtPlateNo") as DropDownList;
                    if (ddlEdtPlateNo != null)
                    {
                        PopVehicles(ddlEdtPlateNo);
                        if (_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].PlateNo != null)
                        {
                            ListItem liI = ddlEdtPlateNo.Items.FindByText(_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.DataSetIndex].PlateNo);
                            if (liI != null)
                                liI.Selected = true;
                        }
                    }
                }
            }
        }
        protected void dgVehicles_UpdateCommand(object source, DataGridCommandEventArgs e)
        {
            int vehicleId = (int)dgVehicles.DataKeys[e.Item.ItemIndex];
            VehicleRequestDetail vehicle;

            if (vehicleId > 0)
                vehicle = _presenter.CurrentVehicleRequest.GetVehicle(vehicleId);
            else
                vehicle = (VehicleRequestDetail)_presenter.CurrentVehicleRequest.VehicleRequestDetails[e.Item.ItemIndex];

            try
            {
                vehicle.VehicleRequest = _presenter.CurrentVehicleRequest;
                DropDownList ddlEdtAssignedVehicle = e.Item.FindControl("ddlEdtAssignedVehicle") as DropDownList;
                vehicle.AssignedVehicle = ddlEdtAssignedVehicle.SelectedValue;
                DropDownList ddlPlateNo = e.Item.FindControl("ddlEdtPlateNo") as DropDownList;
                vehicle.PlateNo = ddlPlateNo.SelectedItem.Text;
                TextBox txtEdtRate = e.Item.FindControl("txtEdtRate") as TextBox;
                vehicle.Rate = Convert.ToDecimal(txtEdtRate.Text);
                TextBox txtEdtDriverPhoneNo = e.Item.FindControl("txtEdtDriverPhoneNo") as TextBox;
                vehicle.DriverPhoneNo = txtEdtDriverPhoneNo.Text;
                TextBox txtEdtRentalDrName = e.Item.FindControl("txtEdtRentalDrName") as TextBox;
                vehicle.RentalDriverName = txtEdtRentalDrName.Text;
                DropDownList ddlEdtCarRental = e.Item.FindControl("ddlEdtCarRental") as DropDownList;
                vehicle.CarRental = _presenter.GetCarRental(Convert.ToInt32(ddlEdtCarRental.SelectedValue));
                DropDownList ddlEdtCarModel = e.Item.FindControl("ddlEdtCarModel") as DropDownList;
                vehicle.CarModel = _presenter.GetCarModel(Convert.ToInt32(ddlEdtCarModel.SelectedValue));
                DropDownList ddlEdtDriver = e.Item.FindControl("ddlEdtDriver") as DropDownList;
                vehicle.AppUser = _presenter.GetUser(Convert.ToInt32(ddlEdtDriver.SelectedValue));

                dgVehicles.EditItemIndex = -1;
                BindVehicles();
                pnlApproval_ModalPopupExtender.Show();
                Master.ShowMessage(new AppMessage("Vehicle Information Successfully Updated", Chai.WorkflowManagment.Enums.RMessageType.Info));
            }
            catch (Exception ex)
            {
                Master.ShowMessage(new AppMessage("Error: Unable to Update Vehicle Information. " + ex.Message, Chai.WorkflowManagment.Enums.RMessageType.Error));
            }
        }
        protected void btnFind_Click(object sender, EventArgs e)
        {
            BindSearchVehicleRequestGrid();
        }
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            try
            {
                if (_presenter.CurrentVehicleRequest.ProgressStatus != ProgressStatus.Completed.ToString())
                {
                    if (ddlApprovalStatus.SelectedValue == "Rejected" && txtRejectedReason.Text == "")
                    {
                        Master.ShowMessage(new AppMessage("Please Insert Rejected/Canceled Reason ", RMessageType.Error));
                        
                    }
                    else
                    {
                        SaveVehicleRequestStatus();
                        _presenter.CurrentVehicleRequest.Project = _presenter.GetProject(Convert.ToInt32(ddlProject.SelectedValue));
                        _presenter.CurrentVehicleRequest.Grant = _presenter.GetGrant(Convert.ToInt32(ddlGrant.SelectedValue));
                        _presenter.SaveOrUpdateVehicleRequest(_presenter.CurrentVehicleRequest);
                        ShowPrint();

                        Master.ShowMessage(new AppMessage("Vehicle Request Approval Processed ", RMessageType.Info));
                        btnApprove.Enabled = false;
                        BindSearchVehicleRequestGrid();
                        pnlApproval_ModalPopupExtender.Show();
                    }
                }
                PrintTransaction();
            }
            catch (Exception ex)
            {
                Master.ShowMessage(new AppMessage("Error! Vehicle Approval not processed due to " + ex.Message, RMessageType.Error));
                ExceptionUtility.LogException(ex, ex.Source);
                ExceptionUtility.NotifySystemOps(ex, _presenter.CurrentUser().FullName);
            }
        }
        protected void grvStatuses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (_presenter.CurrentVehicleRequest.VehicleRequestStatuses != null)
            {
                if (e.Row.RowType == DataControlRowType.DataRow)
                {
                    // e.Row.Cells[1].Text = _presenter.GetUser(_presenter.CurrentVehicleRequest.VehicleRequestStatuses[e.Row.RowIndex].Approver).FullName;
                }
            }
        }
        protected void ddlApprovalStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlApprovalStatus.SelectedValue == "Rejected" || ddlApprovalStatus.SelectedValue == "Canceled")
            {
                lblRejectedReason.Visible = true;
                txtRejectedReason.Visible = true;
                rfvRejectedReason.Enabled = true;
                pnlApproval_ModalPopupExtender.Show();
            }
            else
            {
                lblRejectedReason.Visible = false;
                txtRejectedReason.Visible = false;
                pnlApproval_ModalPopupExtender.Show();
            }
        }
        protected void ddlProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            PopGrants(Convert.ToInt32(ddlProject.SelectedValue));
        }
        protected void ddlDriver_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlDriver = (DropDownList)sender;
            TextBox txtPhoneNo = ddlDriver.FindControl("txtFDriverPhoneNo") as TextBox;
            if (_presenter.GetAssignDriver(Convert.ToInt32(ddlDriver.SelectedIndex)) != null)
                txtPhoneNo.Text = _presenter.GetAssignDriver(Convert.ToInt32(ddlDriver.SelectedIndex)).Employee.Phone;
            else
                txtPhoneNo.Text = String.Empty;
            pnlApproval_ModalPopupExtender.Show();
        }
        protected void btnCancelPopup_Click(object sender, EventArgs e)
        {
            pnlApproval.Visible = false;
        }
        private void PrintTransaction()
        {
            lblRequestNoResult.Text = _presenter.CurrentVehicleRequest.RequestNo.ToString();
            lblRequestedDateResult.Text = _presenter.CurrentVehicleRequest.RequestDate.Value.ToShortDateString();
            lblRequesterResult.Text = _presenter.CurrentVehicleRequest.AppUser.FullName;
            lblProjectIdResult.Text = _presenter.CurrentVehicleRequest.Project.ProjectCode;
            lblDepartureTimeResult.Text = _presenter.CurrentVehicleRequest.DepartureTime.ToString();
            lblDepartureDateResult.Text = _presenter.CurrentVehicleRequest.DepartureDate.Value.ToShortDateString();
            lblReturningDateResult.Text = _presenter.CurrentVehicleRequest.ReturningDate.Value.ToShortDateString();
            lblPurposeOfTravelResult.Text = _presenter.CurrentVehicleRequest.PurposeOfTravel;
            lblNoOfPassengersResult.Text = _presenter.CurrentVehicleRequest.NoOfPassengers.ToString();
            lblApprovalStatusResult.Text = _presenter.CurrentVehicleRequest.ProgressStatus;
            lblDestinationResult.Text = _presenter.CurrentVehicleRequest.Destination;

            grvVehcles.DataSource = _presenter.CurrentVehicleRequest.VehicleRequestDetails;
            grvVehcles.DataBind();

            grvStatuses.DataSource = _presenter.CurrentVehicleRequest.VehicleRequestStatuses;
            grvStatuses.DataBind();


        }
        protected void btnPrintTravellog_Click(object sender, EventArgs e)
        {
            Response.Redirect(String.Format("../Request/frmTravelLog.aspx?VehicleRequestId={0}", _presenter.CurrentVehicleRequest.Id));

        }
    }
}