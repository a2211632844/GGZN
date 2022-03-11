using Kingdee.BOS;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using Kingdee.K3.SCM.Purchase.Business.PlugIn;
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
    [Description("生产订单的销售订单号携带到采购申请单")]
    public class XSDDH_TO_CGSQD : AssortReqEdit
    {
        public override void AfterGenerateReqs(List<DynamicObject> reqDatas)
        {
            base.AfterGenerateReqs(reqDatas);
            if (reqDatas == null || reqDatas.Count == 0)
            {
                return;
            }
            //判断当前配套来源单据是什么，这里演示来源单据是销售订单
            if (OpenSourceBill == "PRD_MO")
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("FBILLNO");
                dt.Columns.Add("FSEQ");

                char spliteSymbol_1 = ',';
                char spliteSymbol_2 = '_';

                //根据配套来源行号字段获取源单数据信息
                foreach (var billItem in reqDatas)
                {
                    var entrys = billItem["ReqEntry"] as DynamicObjectCollection;

                    foreach (var entryItem in entrys)
                    {
                        string srcRowInfoItem = Convert.ToString(entryItem["ASSORTBILLSEQ"]);
                        //throw new Exception("123");
                        if (string.IsNullOrWhiteSpace(srcRowInfoItem) == false)
                        {
                            var srcRowInfoItemArr = srcRowInfoItem.Split(spliteSymbol_1);

                            if (srcRowInfoItemArr != null && srcRowInfoItemArr.Length > 0)
                            {
                                foreach (var item in srcRowInfoItemArr)
                                {
                                    var subiItem = item.Split(spliteSymbol_2);
                                     
                                    string itemSrcBillNo = subiItem[0];
                                    string itemSrcSeq = subiItem[1];

                                    DataRow row = dt.NewRow();

                                    row[0] = itemSrcBillNo;
                                    row[1] = itemSrcSeq;
                                    dt.Rows.Add(row);
                                }
                            }
                        }
                    }
                }
                if (dt.Rows.Count == 0)
                {
                    return;
                }

                //批量从数据库过滤取数
                BatchSqlParam sqlParam = new BatchSqlParam("T_PRD_MO", dt);
                sqlParam.TableAliases = "TR";
                sqlParam.AddJoinExpression(" INNER JOIN T_PRD_MOENTRY T ON T.FID=TR.FID ");
                sqlParam.AddWhereExpression("FBILLNO", KDDbType.String, "FBILLNO", "TR");
                sqlParam.AddWhereExpression("FSEQ", KDDbType.Int64, "FSEQ", "T");
                sqlParam.AddWhereExpression("FMATERIALID", KDDbType.Int64, "FMATERIALID", "T");

                string selectFieldSql = @" distinct TR.FBILLNO,T.FSEQ,T.F_SaleNumber,T.FMATERIALID";//F_GGZN_XSDDH   F_SaleNumber

                Dictionary<string, DynamicObject> dbRstValues = new Dictionary<string, DynamicObject>();

                var dbCols = Kingdee.BOS.App.Data.DBUtils.ExecuteDynamicObject(this.Context, sqlParam, selectFieldSql);

                foreach (var item in dbCols)
                {
                    string key = Convert.ToString(item["FBILLNO"]) + spliteSymbol_2 + Convert.ToString(item["FSEQ"]);
                    string MATERIALID = Convert.ToString(item["FMATERIALID"]);
                    dbRstValues[key] = item;
                    dbRstValues[MATERIALID] = item; 
                }


                //批量回填字段
                foreach (var billItem in reqDatas)
                {
                    var entrys = billItem["ReqEntry"] as DynamicObjectCollection;

                    foreach (var entryItem in entrys)
                    {
                        string srcRowInfoItem = Convert.ToString(entryItem["ASSORTBILLSEQ"]);
                        if (string.IsNullOrWhiteSpace(srcRowInfoItem) == false)
                        {
                            var srcRowInfoItemArr = srcRowInfoItem.Split(spliteSymbol_1);

                            if (srcRowInfoItemArr != null && srcRowInfoItemArr.Length > 0)
                            {
                                foreach (var item in srcRowInfoItemArr)
                                {
                                    if (dbRstValues.ContainsKey(item))
                                    {
                                        entryItem["F_GGZN_XSDDH"] = Convert.ToString(dbRstValues[item]["F_SaleNumber"]);
                                    }

                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
