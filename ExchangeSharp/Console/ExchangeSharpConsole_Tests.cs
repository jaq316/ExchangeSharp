﻿/*
MIT LICENSE

Copyright 2017 Digital Ruby, LLC - http://www.digitalruby.com

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ExchangeSharp
{
    public static partial class ExchangeSharpConsole
    {
        private static void Assert(bool expression)
        {
            if (!expression)
            {
                throw new ApplicationException("Test failure, unexpected result");
            }
        }

        private static string GetSymbol(IExchangeAPI api)
        {
            if (api is ExchangeKrakenAPI)
            {
                return api.NormalizeSymbol("XXBTZUSD");
            }
            else if (api is ExchangeBittrexAPI || api is ExchangePoloniexAPI)
            {
                return api.NormalizeSymbol("BTC-LTC");
            }
            else if (api is ExchangeBinanceAPI)
            {
                return api.NormalizeSymbol("ETH-BTC");
            }
            return api.NormalizeSymbol("BTC-USD");
        }

        public static void RunPerformTests(Dictionary<string, string> dict)
        {
            IExchangeAPI[] apis = ExchangeAPI.GetExchangeAPIDictionary().Values.ToArray();
            foreach (IExchangeAPI api in apis)
            {
                // test all public API for each exchange
                try
                {
                    string symbol = GetSymbol(api);

                    IReadOnlyCollection<string> symbols = api.GetSymbols().ToArray();
                    Assert(symbols != null && symbols.Count != 0 && symbols.Contains(symbol, StringComparer.OrdinalIgnoreCase));

                    ExchangeTrade[] trades = api.GetHistoricalTrades(symbol).ToArray();
                    Assert(trades.Length != 0 && trades[0].Price > 0m && trades[0].Amount > 0m);

                    var book = api.GetOrderBook(symbol);
                    Assert(book.Asks.Count != 0 && book.Bids.Count != 0 && book.Asks[0].Amount > 0m &&
                        book.Asks[0].Price > 0m && book.Bids[0].Amount > 0m && book.Bids[0].Price > 0m);

                    trades = api.GetRecentTrades(symbol).ToArray();
                    Assert(trades.Length != 0 && trades[0].Price > 0m && trades[0].Amount > 0m);

                    var ticker = api.GetTicker(symbol);
                    Assert(ticker != null && ticker.Ask > 0m && ticker.Bid > 0m && ticker.Last > 0m &&
                        ticker.Volume != null && ticker.Volume.PriceAmount > 0m && ticker.Volume.QuantityAmount > 0m);

                    try
                    {
                        var candles = api.GetCandles(symbol, 86400, DateTime.UtcNow.Subtract(TimeSpan.FromDays(7.0)), null).ToArray();
                        Assert(candles.Length != 0 && candles[0].ClosePrice > 0m && candles[0].HighPrice > 0m && candles[0].LowPrice > 0m && candles[0].OpenPrice > 0m &&
                            candles[0].HighPrice >= candles[0].LowPrice && candles[0].HighPrice >= candles[0].ClosePrice && candles[0].HighPrice >= candles[0].OpenPrice &&
                            !string.IsNullOrWhiteSpace(candles[0].Name) && candles[0].ExchangeName == api.Name && candles[0].PeriodSeconds == 86400 && candles[0].VolumePrice > 0.0 &&
                            candles[0].VolumeQuantity > 0.0 && candles[0].WeightedAverage >= 0m);
                    }
                    catch (NotSupportedException)
                    {

                    }
                    catch (NotImplementedException)
                    {

                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Request failed, api: {0}, error: {1}", api, ex.Message);
                }
            }
        }
    }
}
