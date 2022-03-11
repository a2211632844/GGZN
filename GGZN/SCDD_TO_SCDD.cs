using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using Kingdee.K3.Core.MFG.EntityHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGZN
{
    [HotUpdate]
    [Description("生产订单到下级生产订单")]
    public class SCDD_TO_SCDD : AbstractBillPlugIn
    {
        public override void AfterCreateModelData(EventArgs e)
        {
            base.AfterCreateModelData(e);
            //DynamicObjectCollection moEntryDatas = this.View.Model.DataObject.GetDynamicValue("CONST_PRD_MO.CONST_FTreeEntity.ENTITY_ORM_TreeEntity",);
           
            Entity entity = this.Model.BusinessInfo.GetEntity("FTreeEntity");
            DynamicObjectCollection entityObjs = this.Model.GetEntityDataObject(entity);
            
            string Saler = "";
            DynamicObject dys =  this.Model.DataObject;
            //Saler = dys["F_GGZN_Saler"].ToString();
            var  moEntryDatas = this.View.Model.DataObject.GetDataEntityType();

           
            string FormID = this.Model.BusinessInfo.GetForm().Id;
            if (FormID == "PRD_MO")
            {
                string YWYID = "";
                foreach (var item in entityObjs)
                {
                    //string ss = item["F_SaleNumber"].ToString(); //销售订单号
                    string srcbilltype = item["SrcBillType"].ToString();
                    if (srcbilltype== "PRD_MO") 
                    {

                        string FEntityID = item["SrcBillEntryId"].ToString();//源单单据体内码
                        #region
                        string countsql = string.Format(@"select * from T_PRD_MOENTRY_LK MOLK
                                                    JOIN T_PRD_MOENTRY MOE ON MOE.FENTRYID = MOLK.FSID
                                                    JOIN T_PRD_MO MO ON MO.FID = MOE.FID
                                                    WHERE FSID ={0}
                                                     and FRULEID='MO2MO'", FEntityID);
                        DataSet dsc = DBServiceHelper.ExecuteDataSet(Context, countsql);
                        DataTable dtc = dsc.Tables[0];
                        if (dtc.Rows.Count > 0)
                        {
                            this.View.ShowMessage("该单据已经生成过下级生产订单了!", MessageBoxOptions.OKCancel, new Action<MessageBoxResult>(result =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    // 用户选择了OK 
                                    this.View.Close();
                                }
                                else if (result == MessageBoxResult.Cancel)
                                {
                                    // 用户选择了取消      
                                    this.View.Close();
                                }
                            }));
                        }
                        else
                        #endregion
                        {
                            string YWY = string.Format(@"select MO.F_GGZN_SALER from T_PRD_MOENTRY MOE
                                                JOIN T_PRD_MO MO ON MO.FID = MOE.FID
                                                where FENTRYID = {0}", FEntityID);
                            DataSet ds1 = DBServiceHelper.ExecuteDataSet(Context, YWY);
                            DataTable dt1 = ds1.Tables[0];

                            string BTYWY = string.Format("select F_SALER from T_PRD_MOENTRY where FENTRYID = {0}", FEntityID);
                            DataSet ds2 = DBServiceHelper.ExecuteDataSet(Context, BTYWY);
                            DataTable dt2 = ds2.Tables[0];
                            if (dt1.Rows.Count > 0 && dt1.Rows[0][0].ToString() != "0")//表头又业务员
                            {
                                YWYID = dt1.Rows[0][0].ToString();
                                string sql = string.Format("select F_SALENUMBER,F_GGZN_DeliveryDate from T_PRD_MOENTRY where FENTRYID = {0}", FEntityID);
                                DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
                                DataTable dt = ds.Tables[0];
                                if (dt.Rows.Count > 0)
                                {
                                    string Sal_OrderId = dt.Rows[0]["F_SALENUMBER"].ToString();
                                    string FSeq = item["Seq"].ToString();//单据体序号
                                    int row = Convert.ToInt32(FSeq);
                                    this.View.Model.SetValue("F_GGZN_DeliveryDate",dt.Rows[0]["F_GGZN_DeliveryDate"].ToString(),row-1);
                                    this.View.Model.SetValue("F_SaleNumber", Sal_OrderId, row - 1);
                                    this.View.Model.SetValue("F_Saler", YWYID, row - 1);
                                    this.View.Model.SetValue("F_GGZN_SCDDNO", FEntityID, row - 1);
                                    string updatesql = string.Format(@"/*dialect*/ update T_PRD_MOENTRY set F_GGZN_ZT='已生成' from T_PRD_MOENTRY where FENTRYID = '{0}'", FEntityID);
                                    DBUtils.Execute(this.Context, updatesql);
                                }
                            }
                            else if (dt2.Rows.Count > 0) //表体有业务员
                            {
                                YWYID = dt2.Rows[0][0].ToString();
                                string sql = string.Format("select F_SALENUMBER,F_GGZN_DeliveryDate from T_PRD_MOENTRY where FENTRYID = {0}", FEntityID);
                                DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sql);
                                DataTable dt = ds.Tables[0];
                                if (dt.Rows.Count > 0)
                                {
                                    string Sal_OrderId = dt.Rows[0]["F_SALENUMBER"].ToString();
                                    string FSeq = item["Seq"].ToString();//单据体序号
                                    int row = Convert.ToInt32(FSeq);
                                    this.View.Model.SetValue("F_GGZN_DeliveryDate", dt.Rows[0]["F_GGZN_DeliveryDate"].ToString(),row-1);
                                    this.View.Model.SetValue("F_SaleNumber", Sal_OrderId, row - 1);
                                    this.View.Model.SetValue("F_Saler", YWYID, row - 1);
                                    this.View.Model.SetValue("F_GGZN_SCDDNO", FEntityID, row - 1);
                                    string updatesql = string.Format(@"/*dialect*/ update T_PRD_MOENTRY set F_GGZN_ZT='已生成' from T_PRD_MOENTRY where FENTRYID = '{0}'", FEntityID);
                                    DBUtils.Execute(this.Context, updatesql);
                                }
                            }
                        }
                    }
                }
                this.Model.SetValue("F_GGZN_Saler", YWYID);
            }

        }
    }
}
