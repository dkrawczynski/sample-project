using System;
using System.Data;
using Mongoose.IDO;
using Mongoose.IDO.Protocol;

[assembly: IDOExtensionClassAssembly("LSC_CofRoHSReport", typeof(LSC.Extensions.LSC_CofRoHSReportExtensionClass))]

namespace LSC.Extensions
{
    [IDOExtensionClass("LSC_CofRoHSReport")]
    public class LSC_CofRoHSReportExtensionClass : IDOExtensionClass
    {
        // CoNumType is NVARCHAR(10) — matches dbo.ExpandKyByType('CoNumType', ...) padding
        private const int CoNumTypeLength = 10;

        [IDOMethod(MethodFlags.None, "Infobar")]
        public int Rpt_LSC_CofRoHSReportSp(
            [Optional] string    CoNum,
            [Optional] short     CoLine,
            [Optional] short     CoRelease,
            [Optional] string    pSite,
                       ref DataSet Results,
            [Output]   ref string Infobar)
        {
            int severity = 0;
            Infobar = string.Empty;

            try
            {
                // Mirror SP pre-processing: ensure 'S' prefix then pad to CoNumType length
                if (!string.IsNullOrEmpty(CoNum) &&
                    !CoNum.StartsWith("S", StringComparison.OrdinalIgnoreCase))
                {
                    CoNum = "S" + CoNum;
                }
                CoNum = (CoNum ?? string.Empty).PadRight(CoNumTypeLength).Substring(0, CoNumTypeLength);

                const string sql = @"
                    SELECT
                        co_ship.co_num      AS CoNum,
                        co_ship.co_line     AS CoLine,
                        co_ship.co_release  AS CoReleaseS,
                        coitem.item         AS item,
                        item.description    AS description,
                        item.revision       AS rev,
                        co_ship.ship_date   AS curDate,
                        co_ship.qty_shipped AS qty,
                        bill_addr.cust_num  AS cust_num,
                        bill_addr.name      AS custName
                    FROM dbo.co_ship AS co_ship
                    LEFT JOIN dbo.coitem AS coitem
                           ON coitem.co_num     = @CoNum
                          AND coitem.co_line    = @CoLine
                          AND coitem.co_release = @CoRelease
                    LEFT JOIN dbo.item AS item
                           ON item.item = coitem.item
                    LEFT JOIN dbo.co AS co
                           ON co.co_num = @CoNum
                    LEFT JOIN dbo.customer AS customer
                           ON customer.cust_num = co.cust_num
                          AND customer.cust_seq = co.cust_seq
                    LEFT JOIN dbo.customer AS bill_cust
                           ON bill_cust.cust_num = co.cust_num
                          AND bill_cust.cust_seq  = 0
                    LEFT JOIN dbo.custaddr AS custaddr
                           ON custaddr.cust_num = customer.cust_num
                          AND custaddr.cust_seq = customer.cust_seq
                    LEFT JOIN dbo.custaddr AS bill_addr
                           ON bill_addr.cust_num = bill_cust.cust_num
                          AND bill_addr.cust_seq  = 0
                    WHERE co_ship.co_num     = @CoNum
                      AND co_ship.co_line    = @CoLine
                      AND co_ship.co_release = @CoRelease";

                IDbCommand cmd = Context.Commands.CreateTextCommand(sql);
                cmd.Parameters.Add(
                    Context.Commands.CreateInputParameter("@CoNum",     DbType.String, CoNum));
                cmd.Parameters.Add(
                    Context.Commands.CreateInputParameter("@CoLine",    DbType.Int16,  CoLine));
                cmd.Parameters.Add(
                    Context.Commands.CreateInputParameter("@CoRelease", DbType.Int16,  CoRelease));

                Results = Context.Commands.ExecuteDataSet(cmd);
            }
            catch (Exception ex)
            {
                severity = 16;
                Infobar  = ex.Message;
            }

            return severity;
        }
    }
}
