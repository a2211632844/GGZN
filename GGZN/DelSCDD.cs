using Kingdee.BOS.App.Data;
using Kingdee.BOS.Core;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGZN
{
    [HotUpdate]
    [Description("删除生产订单")]
    public class DelSCDD : AbstractOperationServicePlugIn
    {
        public override void OnPreparePropertys(PreparePropertysEventArgs e) 
        {
            e.FieldKeys.Add("F_GGZN_SCDDNO");
            e.FieldKeys.Add("TreeEntity");
        }
        public override void BeforeExecuteOperationTransaction(BeforeExecuteOperationTransaction e)
        {
            base.BeforeExecuteOperationTransaction(e);
            if (e.SelectedRows.Count() < 1)
            {
                return;
            }
            foreach (ExtendedDataEntity item in e.SelectedRows) 
            {

            }
            foreach (ExtendedDataEntity extended in e.SelectedRows) 
            {
                if (this.FormOperation.Operation == "Delete") 
                {
                    DynamicObject dy = extended.DataEntity;
                    DynamicObjectCollection docEntity = dy["TreeEntity"] as DynamicObjectCollection;
                    string FMOBILLNO = "";
                    foreach (DynamicObject entity in docEntity)
                    {
                        if (entity["F_GGZN_SCDDNO"].IsNullOrEmptyOrWhiteSpace() == false)
                        {
                            FMOBILLNO = entity["F_GGZN_SCDDNO"].ToString();
                            string updatesql = string.Format(@"/*dialect*/ update T_PRD_MOENTRY set F_GGZN_ZT='' from T_PRD_MOENTRY where FENTRYID = '{0}'", FMOBILLNO);
                            DBUtils.Execute(this.Context, updatesql);
                        }

                    }
                }
                    
            }
        }
    }
}
