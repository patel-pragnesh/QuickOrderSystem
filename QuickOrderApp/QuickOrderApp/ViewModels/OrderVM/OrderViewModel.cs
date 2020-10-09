﻿using Library.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuickOrderApp.Managers;
using QuickOrderApp.Utilities.Attributes;
using QuickOrderApp.Utilities.Loadings;
using QuickOrderApp.Utilities.Presenters;
using QuickOrderApp.Views.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace QuickOrderApp.ViewModels.OrderVM
{
    [QueryProperty("OrderStatus", "status")]
    public class OrderViewModel : BaseViewModel
    {
        public ObservableCollection<OrderPresenter> UserOrders { get; set; }

        public ObservableCollection<ProductPresenter> ProductPresenters { get; set; }
      
        public ICommand GetOrdersCommand => new Command<string>(async (arg) =>
            {

                await Shell.Current.GoToAsync($"{UserOrdersWithStatus.Route}?status={arg}");


    });
      
        public ICommand MoreCommand => new Command(async() => 
        {
            await LoadUserOrderWithStatus(OrderStatus);

           

        });

        private string orderId;

        public string OrderId
        {
            get { return orderId; }
            set
            {
                orderId = value;
                OnPropertyChanged();

            }
        }

        private string orderstatus;

        public string OrderStatus
        {
            get { return orderstatus; }
            set 
            {
                orderstatus = value;

                OnPropertyChanged();

                  

                TokenExpManger tokenExpManger = new TokenExpManger(App.TokenDto.Exp);

                if (tokenExpManger.IsExpired())
                {

                    tokenExpManger.CloseSession();
                    //DisplayNotification();

                }
                else
                {
                    Task.Run(async () =>
                    {

                        await LoadOrders(OrderStatus);
                    }).Wait();


                }
             

            }
        }

        private bool checkoutEnable;

        public bool CheckOutEnable
        {
            get { return checkoutEnable; }
            set
            {
                checkoutEnable = value;
                OnPropertyChanged();
            }
        }

       public LoadingManager LoadingManager { get; set; }

        Dictionary<string, IEnumerable<Order>> KeyValues { get; set; }


        public OrderViewModel()
        {

            PropertiesInitialization();

            MessagingCenter.Subscribe<Order>(this, "RemoveOrderSubtmitedMsg", (sender) =>
            {


                var order = UserOrders.Where(op => op.OrderId == sender.OrderId).FirstOrDefault();

                UserOrders.Remove(order);

            });

            MessagingCenter.Subscribe<OrderPresenter, OrderPresenter>(this, "Refresh", (sender, arg) =>
             {

                 UserOrders.Remove(arg);
               
            });

            //GetOrdersCommand = new Command<string>(async (arg) =>
            //{

            //    await Shell.Current.GoToAsync($"{UserOrdersWithStatus.Route}?status={arg}");
              

            //});
        }


        void PropertiesInitialization()
        {
            UserOrders = new ObservableCollection<OrderPresenter>();
            LoadingManager = new LoadingManager();
            KeyValues = new Dictionary<string, IEnumerable<Order>>();
        }

        public async Task SetOrders()
        {
            var userOrderData = orderDataStore.GetUserOrders(App.LogUser.UserId);

          
            UserOrders.Clear();

            foreach (var item in userOrderData)
            {
               
                item.StoreOrder = await StoreDataStore.GetItemAsync(item.StoreId.ToString());
                var presenter = new OrderPresenter(item);

                UserOrders.Add(presenter);
            }
           
        }


        async Task LoadOrders(string value)
        {
            LoadingManager.OnLoading();
            Status _statusvalue = (Status)Enum.Parse(typeof(Status), value);

            var orderData = await orderDataStore.GetOrdersOfUserWithSpecificStatus(App.LogUser.UserId, _statusvalue, App.TokenDto.Token);


            KeyValues.Clear();
           
            KeyValues.Add("orderAdded", orderData);

            UserOrders.Clear();

            foreach (var item in orderData)
            {

                item.StoreOrder = await StoreDataStore.GetStoreInformation(item.StoreId);
                var presenter = new OrderPresenter(item);

                UserOrders.Add(presenter);
            }

            LoadingManager.OffLoading();
        }


      
        async Task LoadUserOrderWithStatus(string value)
        {
            LoadingManager.OnLoading();
            Status _statusvalue = (Status)Enum.Parse(typeof(Status), value);

            var orderData = await orderDataStore.GetOrdersOfUserWithSpecificStatus(App.LogUser.UserId, _statusvalue,App.TokenDto.Token);

            if (!KeyValues.ContainsKey("orderAdded"))
            {

            KeyValues.Add("orderAdded", orderData);
            }

           
            switch (_statusvalue)
            {

                case Status.Completed:
                    {

                        List<Order> tempData = new List<Order>();

                        var data = await orderDataStore.GetOrdersOfUserWithSpecificStatusDifferent(KeyValues["orderAdded"], _statusvalue,App.LogUser.UserId);

                        if (data != null)
                        {
                            foreach (var item in KeyValues["orderAdded"])
                            {
                                if (!tempData.Any(o => o.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);
                                }
                            }

                            foreach (var item in data)
                            {
                                if (!tempData.Any(s => s.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);

                                }

                            }

                            KeyValues.Clear();
                            KeyValues.Add("orderAdded", tempData);


                            foreach (var item in KeyValues["orderAdded"])
                            {

                                if (!UserOrders.Any(s => s.OrderId == item.OrderId))
                                {

                                  

                                    item.StoreOrder = await StoreDataStore.GetStoreInformation(item.StoreId);

                                    var presenter = new OrderPresenter(item);

                                    UserOrders.Add(presenter);

                                }
                            }
                        }



                        break;
                    }

                case Status.NotSubmited:
                    {

                        List<Order> tempData = new List<Order>();

                        var data = await orderDataStore.GetOrdersOfUserWithSpecificStatusDifferent(KeyValues["orderAdded"],_statusvalue,App.LogUser.UserId);

                        if (data != null)
                        {
                            foreach (var item in KeyValues["orderAdded"])
                            {
                                if (!tempData.Any(o => o.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);
                                }
                            }

                            foreach (var item in data)
                            {
                                if (!tempData.Any(s => s.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);

                                }

                            }

                            KeyValues.Clear();
                            KeyValues.Add("orderAdded", tempData);


                            foreach (var item in KeyValues["orderAdded"])
                            {

                                if (!UserOrders.Any(s => s.OrderId == item.OrderId))
                                {

                                    item.StoreOrder = await StoreDataStore.GetStoreInformation(item.StoreId);
                                    var presenter = new OrderPresenter(item);

                                    UserOrders.Add(presenter);

                                }
                            }
                        }

                        break;

                      

                    }
                case Status.Submited:
                    {


                        List<Order> tempData = new List<Order>();

                        var data = await orderDataStore.GetOrdersOfUserWithSpecificStatusDifferent(KeyValues["orderAdded"], _statusvalue,App.LogUser.UserId);

                        if (data != null)
                        {
                            foreach (var item in KeyValues["orderAdded"])
                            {
                                if (!tempData.Any(o => o.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);
                                }
                            }

                            foreach (var item in data)
                            {
                                if (!tempData.Any(s => s.OrderId == item.OrderId))
                                {

                                    tempData.Add(item);

                                }

                            }

                            KeyValues.Clear();
                            KeyValues.Add("orderAdded", tempData);


                            foreach (var item in KeyValues["orderAdded"])
                            {

                                if (!UserOrders.Any(s => s.OrderId == item.OrderId))
                                {

                                    item.StoreOrder = await StoreDataStore.GetStoreInformation(item.StoreId);
                                    var presenter = new OrderPresenter(item);

                                    UserOrders.Add(presenter);

                                }
                            }
                        }


                      

                        break;
                    }
                default:
                    break;
            }

            LoadingManager.OffLoading();


        }


    }
}
