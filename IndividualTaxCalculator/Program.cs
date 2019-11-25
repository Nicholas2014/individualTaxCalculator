using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace IndividualTaxCalculator
{
    class Program
    {
        /// <summary>
        /// 2019个税税率表
        /// (] 左开右闭
        /// </summary>
        static List<TaxLevel> taxGradeList = new List<TaxLevel>();
        /// <summary>
        /// 1-12月每月应缴的个税金额
        /// month,tax,totalTax（逐月累计额）
        /// </summary>
        static Dictionary<int, decimal> taxInMonth = new Dictionary<int, decimal>();
        private static readonly int BEGIN_TAX = 5000;
        static Program()
        {
            taxGradeList.AddRange(new List<TaxLevel>()
            {
                new TaxLevel() {Level = 1, Min = Int32.MinValue, Max = 36000, Rate = 3, Deduct = 0},
                new TaxLevel() {Level = 2, Min = 36000, Max = 144000, Rate = 10, Deduct = 2520},
                new TaxLevel() {Level = 3, Min = 144000, Max = 300000, Rate = 20, Deduct = 16920},
                new TaxLevel() {Level = 4, Min = 300000, Max = 420000, Rate = 25, Deduct = 31920},
                new TaxLevel() {Level = 5, Min = 420000, Max = 660000, Rate = 30, Deduct = 52920},
                new TaxLevel() {Level = 6, Min = 660000, Max = 960000, Rate = 35, Deduct = 85920},
                new TaxLevel() {Level = 7, Min = 960000, Max = Int32.MaxValue, Rate = 45, Deduct = 181920}
            });
        }

        static void Main(string[] args)
        {
            Console.WriteLine("=============== 个税计算器 ===============");
            Console.WriteLine("输入应扣税额（月工资）：");
            var salary = Console.ReadLine();
            Console.WriteLine("输入社保缴费额：");
            var social = Console.ReadLine();
            Console.WriteLine("输入专项费用扣除：");
            var cutOff = Console.ReadLine();
            Calculate(Convert.ToDecimal(salary), Convert.ToDecimal(social), Convert.ToDecimal(cutOff));
            Console.WriteLine("计算完毕：");
            foreach (var item in taxInMonth)
            {
                Console.WriteLine($"{item.Key,3}月税金：{item.Value,-10}");
            }

            Console.WriteLine();
            Console.WriteLine($"{taxInMonth.Sum(r => r.Value),18}");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// 计算1-12月个月个税扣除额
        /// </summary>
        /// <param name="salaryMonth">月薪</param>
        /// <param name="socialAmount">社保缴费额</param>
        /// <param name="specialDeductAmount">专项扣除额</param>
        /// <returns></returns>
        static void Calculate(decimal salaryMonth, decimal socialAmount, decimal specialDeductAmount)
        {
            // 应扣税额 = 月薪 - 社保
            var srcDeductAmount = salaryMonth - socialAmount;
            var totalDeductAmount = 0;
            foreach (var month in Enumerable.Range(1, 12))
            {
                // 税金 = （应扣税额 - 专项费用扣除 - 个税起征点）*对应的扣税比例 - 速算扣除数
                // 从2019年1月1日起，个税扣税实行年度累计扣税 即
                // 税金 = （Sum(应扣税额) - Sum(专项费用扣除) - Sum(个税起征点)）*对应的扣税比例 - 速算扣除数 - Sum(税金)
                var amount = (srcDeductAmount * month - specialDeductAmount * month - BEGIN_TAX * month);
                var res = GetRate(amount);
                var tax = amount * (res.Rate * 1.0m / 100) - res.Deduct - taxInMonth.Where(r => r.Key < month).Sum(r => r.Value);
                if (!taxInMonth.ContainsKey(month))
                {
                    taxInMonth.Add(month, tax);
                }
                else
                {
                    taxInMonth[month] = tax;
                }
            }
        }

        static (int Rate, int Deduct) GetRate(decimal amount)
        {
            foreach (var item in taxGradeList)
            {
                if (amount >= item.Min && amount < item.Max)
                {
                    return (item.Rate, item.Deduct);
                }
            }

            return (1, 0);
        }
    }

    internal class TaxLevel
    {
        public int Level { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        /// <summary>
        /// 预扣率
        /// </summary>
        public int Rate { get; set; }
        /// <summary>
        /// 速算扣除数
        /// </summary>
        public int Deduct { get; set; }
    }
}
