/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Brokerages;
using QuantConnect.Indicators;
using QuantConnect.Orders;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// The demonstration algorithm shows some of the most common order methods when working with Crypto assets.
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="using quantconnect" />
    /// <meta name="tag" content="trading and orders" />
    public class JsboigeCryptoAlgorithm : QCAlgorithm, IRegressionAlgorithmDefinition
    {
        private ExponentialMovingAverage _fast;
        private ExponentialMovingAverage _slow;

        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2018, 1, 1); // Set Start Date
            SetEndDate(2018, 12, 31); // Set End Date

            // Although typically real brokerages as GDAX only support a single account currency,
            // here we add both USD and EUR to demonstrate how to handle non-USD account currencies.
            // Set Strategy Cash (USD)
            SetCash(10000);

            
            // Add some coins as initial holdings
            // When connected to a real brokerage, the amount specified in SetCash
            // will be replaced with the amount in your actual account.
            SetCash("BTC", 1m);

            SetBrokerageModel(BrokerageName.GDAX, AccountType.Cash);

            // You can uncomment the following line when live trading with GDAX,
            // to ensure limit orders will only be posted to the order book and never executed as a taker (incurring fees).
            // Please note this statement has no effect in backtesting or paper trading.
            // DefaultOrderProperties = new GDAXOrderProperties { PostOnly = true };

            // Find more symbols here: http://quantconnect.com/data
           
            var symbol = AddCrypto("BTCUSD").Symbol;

            // create two moving averages
            _fast = EMA(symbol, 30, Resolution.Minute);
            _slow = EMA(symbol, 60, Resolution.Minute);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (Portfolio.CashBook["BTC"].ConversionRate == 0)
            {
                Log($"BTC conversion rate: {Portfolio.CashBook["BTC"].ConversionRate}");

                throw new Exception("Conversion rate is 0");
            }
            if (Time.Hour == 2 && Time.Minute == 0)
            {
                // Submit a buy limit order for BTC at 5% below the current price
                var usdTotal = Portfolio.CashBook["USD"].Amount;
                var limitPrice = Math.Round(Securities["BTCUSD"].Price * 0.95m, 2);
                // use only half of our total USD
                var quantity = usdTotal * 0.5m / limitPrice;
                LimitOrder("BTCUSD", quantity, limitPrice);
            }
            else if (Time.Hour == 11 && Time.Minute == 0)
            {
                // Liquidate our BTC holdings (including the initial holding)
                SetHoldings("BTCUSD", 0m);
            }
        }

        public override void OnOrderEvent(OrderEvent orderEvent)
        {
            Debug(Time + " " + orderEvent);
        }

        public override void OnEndOfAlgorithm()
        {
            Log($"{Time} - TotalPortfolioValue: {Portfolio.TotalPortfolioValue}");
            Log($"{Time} - CashBook: {Portfolio.CashBook}");
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// Data Points count of all timeslices of algorithm
        /// </summary>
        public long DataPoints => 12965;

        /// <summary>
        /// Data Points count of the algorithm history
        /// </summary>
        public int AlgorithmHistoryDataPoints => 240;

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "10"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "0%"},
            {"Drawdown", "0%"},
            {"Expectancy", "0"},
            {"Net Profit", "0%"},
            {"Sharpe Ratio", "0"},
            {"Probabilistic Sharpe Ratio", "0%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "0"},
            {"Beta", "0"},
            {"Annual Standard Deviation", "0"},
            {"Annual Variance", "0"},
            {"Information Ratio", "0"},
            {"Tracking Error", "0"},
            {"Treynor Ratio", "0"},
            {"Total Fees", "$85.34"},
            {"Estimated Strategy Capacity", "$0"},
            {"Lowest Capacity Asset", "BTCEUR XJ"},
            {"Portfolio Turnover", "118.08%"},
            {"OrderListHash", "1bf1a6d9dd921982b72a6178f9e50e68"}
        };
    }
}
