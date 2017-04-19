using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitcoins
{
    class BitcoinTransaction
    {
        Network BitcoinNetwork;
        BitcoinSecret PrivateKey;
        QBitNinjaClient Client;
        Transaction CurrentTransaction;
        List<ICoin> ReceivedCoins;

        public BitcoinTransaction()
        {
            Initialise();
        }

        public BitcoinTransaction(string key)
        {
            Initialise();
            ImportPrivateKey(key);
        }

        void Initialise()
        {
            BitcoinNetwork = Network.TestNet;
            PrivateKey = GetPrivateKey();
        }

        BitcoinSecret GetPrivateKey()
        {
            var privateKey = new Key();
            return privateKey.GetWif(BitcoinNetwork);
        }

        public void ImportPrivateKey(string key)
        {
            PrivateKey = new BitcoinSecret(key);
            BitcoinNetwork = PrivateKey.Network;
        }

        public void Send(string toAddress, decimal amount, string message)
        {
            var response = GetResponse();
            ReceivedCoins = response.ReceivedCoins;
            var outpointToSpend = GetOutpointToSpend();

            CurrentTransaction.Inputs.Add(new TxIn()
            {
                PrevOut = outpointToSpend
            });

            var address = new BitcoinPubKeyAddress(toAddress);
            var transactionAmount = new Money(amount, MoneyUnit.BTC);

            var minerFee = GetMinerFee();

            var txInAmount = (Money)ReceivedCoins[(int)outpointToSpend.N].Amount;
            Money changeBackAmount = txInAmount - transactionAmount - minerFee;


            TxOut transactionTxOut = new TxOut()
            {
                Value = transactionAmount,
                ScriptPubKey = address.ScriptPubKey,
            };

            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = PrivateKey.ScriptPubKey
            };

            CurrentTransaction.Outputs.Add(transactionTxOut);
            CurrentTransaction.Outputs.Add(changeBackTxOut);

            AddMessage(message);

            Sign();
        }

        private Money GetMinerFee() => new Money((decimal)0.0001, MoneyUnit.BTC);

        private OutPoint GetOutpointToSpend()
        {
            foreach (var coin in ReceivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == PrivateKey.ScriptPubKey)
                    return coin.Outpoint;
            }

            throw new Exception("Transaction doesn't contain specified ScriptPubKey");

        }

        private void AddMessage(string message)
        {
            var msgBytes = Encoding.UTF8.GetBytes(message);
            CurrentTransaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(msgBytes)
            });
        }

        private void Sign()
        {
            CurrentTransaction.Inputs[0].ScriptSig = PrivateKey.ScriptPubKey;
            CurrentTransaction.Sign(PrivateKey, false);
        }

        public GetTransactionResponse GetResponse()
        {
            var client = new QBitNinjaClient(BitcoinNetwork);
            var transactionID = uint256.Parse("e44587cf08b4f03b0e8b4ae7562217796ec47b8c91666681d71329b764add2e3");
            return client.GetTransaction(transactionID).Result;
        }

    }
}
