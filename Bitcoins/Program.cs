
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitcoins
{
    class Program
    {
        static void Main(string[] args)
        {
            var transaction = new BitcoinTransaction();

            transaction.Send("test", 0.004m, "test messag");


            //Get transaction info
            var transactionResponse = transaction.GetResponse();
            Console.WriteLine(transactionResponse.TransactionId);
            Console.WriteLine(transactionResponse.Block.Confirmations);

            Console.ReadLine();
        }


    }
}
