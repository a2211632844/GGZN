using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
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
    [Description("委外工序退货单退货数量反写委外工序发料单")]
    public class WWGXTH_TO_WWGXFL:AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e)
        {
            base.OnPreparePropertys(e);
            e.FieldKeys.Add("FEntity");
            e.FieldKeys.Add("F_GGZN_ReturnQty");
            e.FieldKeys.Add("F_GGZN_FLFENTRYID");
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            foreach (ExtendedDataEntity item in e.SelectedRows)
            {

            }
            foreach (ExtendedDataEntity extended in e.SelectedRows)
            {
                DynamicObject dy = extended.DataEntity;
                DynamicObjectCollection docEntity = dy["FEntity"] as DynamicObjectCollection;

                foreach (DynamicObject entity in docEntity)
                {
                    if (this.FormOperation.Operation == "Delete")
                    {
                        string THQty = entity["F_GGZN_ReturnQty"].ToString();//退货数量
                        string RKYDNM = entity["F_GGZN_FLFENTRYID"].ToString();//源单内码(入库的源单编号)

                        string sql = $@"/*dialect*/  update T_SUB_GXWWFLEntity set F_GGZN_RETURNQTY =FLE.F_GGZN_RETURNQTY - {THQty}, F_GGZN_UNStockQty =FLE.F_GGZN_UNStockQty-{THQty},F_GGZN_TrueInstockQty = FLE.F_GGZN_TrueInstockQty+{THQty}
                                                from T_SUB_GXWWFLEntity FLE 
                                                join T_SUB_GXWWTHEntity THE on FLE.FEntryID = THE.F_GGZN_FLFEntryId
                                                WHERE FLE.FEntryID = {RKYDNM}";
                        DBUtils.Execute(Context, sql);
                    }
                }
            }
        }

        public override void EndOperationTransaction(EndOperationTransactionArgs e)
        {
            base.EndOperationTransaction(e);
            foreach (DynamicObject extended in e.DataEntitys)
            {
                DynamicObject dy = extended;
                DynamicObjectCollection docEntity = dy["FEntity"] as DynamicObjectCollection;
               
                foreach (DynamicObject entity in docEntity) 
                {
                    if (this.FormOperation.Operation == "Save")
                    {
                        string THQty = entity["F_GGZN_ReturnQty"].ToString();//退货数量
                        string RKYDNM = entity["F_GGZN_FLFENTRYID"].ToString();//源单内码(入库的源单编号)
                        string sqlsql = $@" /*dialect*/ select FLE.F_GGZN_UNStockQty
                                            from T_SUB_GXWWFLEntity FLE 
                                            join T_SUB_GXWWTHEntity THE on FLE.FEntryID = THE.F_GGZN_FLFEntryId
                                            WHERE FLE.FEntryID = {RKYDNM}";
                        DataSet ds = DBServiceHelper.ExecuteDataSet(Context, sqlsql);
                        DataTable dt = ds.Tables[0];
                        string SYRKSL = dt.Rows[0][0].ToString();

                        string ecxtSQL = $@"/*dialect*/ 
                                                    select F_GGZN_FLFENTRYID from T_SUB_GXWWTHEntity
                                                    where F_GGZN_FLFENTRYID = {RKYDNM}"; 
                        DataSet ecxtds = DBServiceHelper.ExecuteDataSet(Context, ecxtSQL);
                        DataTable ecxtdt = ecxtds.Tables[0];
                        if (ecxtdt.Rows.Count > 1)//二次下推
                        {
                            string sql = $@"/*dialect*/  update T_SUB_GXWWFLEntity set 
                                                 F_GGZN_RETURNQTY ={THQty}+FLE.F_GGZN_RETURNQTY
                                                ,F_GGZN_UNStockQty=FLE.F_GGZN_UNStockQty+{THQty}
                                                ,F_GGZN_TrueInstockQty = FLE.F_GGZN_TrueInstockQty-{THQty}
                                                from T_SUB_GXWWFLEntity FLE 
                                                join T_SUB_GXWWTHEntity THE on FLE.FEntryID = THE.F_GGZN_FLFEntryId
                                                WHERE FLE.FEntryID = {RKYDNM}";
                            //throw new Exception(sql);
                            DBUtils.Execute(Context, sql);
                        }
                        else //第一次下推
                        {
                            if (Convert.ToDecimal(SYRKSL) > Convert.ToDecimal(THQty)) //剩余入库数量》退货数量
                            {
                                string sql = $@"/*dialect*/  update T_SUB_GXWWFLEntity set 
                                                 F_GGZN_RETURNQTY ={THQty}
                                                ,F_GGZN_UNStockQty=FLE.F_GGZN_TRUEQTY-RKE.F_GGZN_INSTOCKQTY+{THQty}
                                                ,F_GGZN_TrueInstockQty = RKE.F_GGZN_INSTOCKQTY-{THQty}
                                                from T_SUB_GXWWFLEntity FLE 
                                                join T_SUB_GXWWTHEntity THE on FLE.FEntryID = THE.F_GGZN_FLFEntryId
                                                JOIN T_SUB_GXWWRKEntity RKE ON RKE.F_GGZN_YDNM = THE.F_GGZN_FLFEntryId
                                                WHERE FLE.FEntryID = {RKYDNM}";
                                //throw new Exception(sql);
                                DBUtils.Execute(Context, sql);
                            }
                        }
                    }
                    //else if(this.FormOperation.Operation == "Delete")
                    //{
                    //    string THQty = entity["F_GGZN_ReturnQty"].ToString();//退货数量
                    //    string RKYDNM = entity["F_GGZN_FLFENTRYID"].ToString();//源单内码(入库的源单编号)

                    //    string sql = $@"/*dialect*/  update T_SUB_GXWWFLEntity set F_GGZN_RETURNQTY =FLE.F_GGZN_RETURNQTY - {THQty}, F_GGZN_UNStockQty =FLE.F_GGZN_UNStockQty-{THQty},F_GGZN_TrueInstockQty = FLE.F_GGZN_TrueInstockQty+{THQty}
                    //                            from T_SUB_GXWWFLEntity FLE 
                    //                            join T_SUB_GXWWTHEntity THE on FLE.FEntryID = THE.F_GGZN_FLFEntryId
                    //                            WHERE FLE.FEntryID = {RKYDNM}";
                    //    DBUtils.Execute(Context, sql);
                    //}
                }
            }
        }
       
    }
}
