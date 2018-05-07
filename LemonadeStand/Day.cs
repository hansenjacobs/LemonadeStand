﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LemonadeStand
{
    class Day
    {
        static int numberOfDaysToforecastWeather = 7;
        static int minPotentialCustomers = 0;
        static int maxPotentialCustomers = 175;

        List<Weather> forecast;
        Random random;
        List<Recipe> recipes;
        List<Player> players;
        List<Customer> customers;
        int dayNumber;

        public Day(Random random, List<Player> players, int dayNumber)
        {
            this.random = random;
            Forecast = new List<Weather>();
            CreateForecast();
            recipes = new List<Recipe>();
            customers = new List<Customer>();
            CreateCustomers();
            this.players = players;
            this.dayNumber = dayNumber;
        }    

        public Day(Random random, List<Player> players, List<Weather> previousforecast, int dayNumber)
        {
            this.random = random;
            Forecast = new List<Weather>();
            Forecast.AddRange(previousforecast);
            Forecast.RemoveAt(0);
            Updateforecast();
            recipes = new List<Recipe>();
            customers = new List<Customer>();
            CreateCustomers();
            this.players = players;
            this.dayNumber = dayNumber;
        }

        public List<Weather> Forecast
        {
            get { return forecast; }
            set { forecast = value; }
        }

        private int CalculateCustomerCount()
        {
            double output = random.Next(minPotentialCustomers, maxPotentialCustomers);
            output = output * (forecast[0].TempratureHigh / Weather.MaxTempratureHigh);
            output = output / (forecast[0].ConditionIndex + 1);
            return Convert.ToInt16(Convert.ToDouble(random.Next(minPotentialCustomers, maxPotentialCustomers)) * (Convert.ToDouble(forecast[0].TempratureHigh) / Convert.ToDouble(Weather.MaxTempratureHigh)) / Convert.ToDouble((forecast[0].ConditionIndex + 1)));
        }

        private void CreateCustomers()
        {
            int customerCount = CalculateCustomerCount();
            for(int i = 0; i < customerCount; i++)
            {
                customers.Add(new Customer(random, forecast[0]));
            }
        }

        private void CreateForecast()
        {
            while (Forecast.Count != numberOfDaysToforecastWeather)
            {
                if (Forecast.Count < numberOfDaysToforecastWeather)
                {
                    Forecast.Add(new Weather(random));
                }
                else
                {
                    Forecast.RemoveAt(Forecast.Count - 1);
                }
            }
        }

        public void SetPlayerRecipes(List<Product> products)
        {
            for (int i = 0; i < players.Count; i++)
            {
                recipes.Add(new Recipe(products));
                recipes[i].SetRecipe(players[i].Name);
            }
        }

        private void SimulateCustomers(Player player, int playerIndex)
        {
            int cupsRemainingInPitcher = 0;

            foreach (Customer customer in customers)
            {
                if (customer.DoesPurchase(forecast[0], recipes[playerIndex]))
                {
                    if (cupsRemainingInPitcher > 0 && player.Invetory["cup"] > 0 && player.Invetory["ice cube"] >= recipes[playerIndex].IceCubeCount)
                    {
                        player.DayDetails[dayNumber].RecordPurchase(recipes[playerIndex].SellPrice);
                        cupsRemainingInPitcher--;
                    }
                    else if (player.Invetory["cup"] > 0 && player.Invetory["ice cube"] >= recipes[playerIndex].IceCubeCount)
                    {
                        if (player.MakePitcher(recipes[playerIndex]))
                        {
                            cupsRemainingInPitcher = Recipe.CupsPerPitcher;
                            player.DayDetails[dayNumber].RecordPurchase(recipes[playerIndex].SellPrice);
                            cupsRemainingInPitcher--;
                        }
                        else
                        {
                            player.DayDetails[dayNumber].RanOutOfInventory = true;
                            break;
                        }
                    }
                    else
                    {
                        player.DayDetails[dayNumber].RanOutOfInventory = true;
                        break;
                    }

                }

            }
        }

        public void SimulateDay()
        {
            for(int i = 0; i < players.Count; i++)
            {
                Player player = players[i];
                player.DayDetails.Insert(dayNumber, new DayDetails());
                player.DayDetails[dayNumber].BankAccountStartingBalance = player.BankBalance;
                player.DayDetails[dayNumber].BankAccountEndingBalance = player.BankBalance;
                player.DayDetails[dayNumber].PotentialCustomerCount = customers.Count;

                SimulateCustomers(player, i);

                UI.DisplayPlayerDayResults(player, dayNumber + 1);

            }
        }

        private void Updateforecast()
        {
            for(int i = 0; i < Forecast.Count; i++)
            {
                Forecast[i].AlterForecast();
            }

            CreateForecast();
        }
        

    }
}
