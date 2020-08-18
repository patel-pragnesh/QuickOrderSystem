﻿using Library.Models;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using QRCoder;
using QuickOrderApp.Utilities.Presenters;
using QuickOrderApp.Utilities.Static;
using Stripe;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;


namespace QuickOrderApp.ViewModels.OrderVM
{
    public class UserDetailOrderViewModel : BaseViewModel
    {
        public ObservableCollection<ProductPresenter> ProductPresenters { get; set; }
        public ICommand CheckoutCommand { get; set; }

        private double total;

        private int orderQuantity;

        public int OrderQuantity
        {
            get { return orderQuantity; }
            set { orderQuantity = value;
                OnPropertyChanged();
            }
        }

        private double totalTax;

        public double TotalTax
        {
            get { return totalTax; }
            set { totalTax = value;
                OnPropertyChanged();
            }
        }

        private double stripefee;

        public double StripeFee
        {
            get { return stripefee; }
            set { stripefee = value;
                OnPropertyChanged();
            }
        }




        public double Total
        {
            get { return total; }
            set
            {
                total = value;
                OnPropertyChanged();
            }
        }

        private bool ispickup;

        public bool IspickUp
        {
            get { return ispickup; }
            set
            {
                ispickup = value;
                OnPropertyChanged();

                if (IspickUp)
                {
                    IsDelivery = false;
                    OrderTypeFee = 0;

                    if (isDeliveryFeeAdded)
                    {
                        Total -= 5;
                    }

                }
            }
        }

        bool isDeliveryFeeAdded;

        private bool isdelivery;

        public bool IsDelivery
        {
            get { return isdelivery; }
            set
            {
                isdelivery = value;
                OnPropertyChanged();

                if (IsDelivery)
                {
                    IspickUp = false;

                    OrderTypeFee = 5;
                    Total += 5;
                    isDeliveryFeeAdded = true;

                }
            }
        }


        private double ordertypefee;

        public double OrderTypeFee
        {
            get { return ordertypefee; }
            set
            {
                ordertypefee = value;
                OnPropertyChanged();
            }
        }

        private Library.Models.Order orderdetail;

        public Library.Models.Order OrderDetail
        {
            get { return orderdetail; }
            set
            {
                orderdetail = value;
                OnPropertyChanged();
            }
        }


        private ImageSource qrcodeimg;

        public ImageSource QRCodeImg
        {
            get { return qrcodeimg; }
            set { qrcodeimg = value;
                OnPropertyChanged();
            }
        }


        public UserDetailOrderViewModel()
        {         

            ProductPresenters = new ObservableCollection<ProductPresenter>();
            OrderDetail = SelectedOrder.CurrentOrder;
            OrderQuantity = OrderDetail.OrderProducts.Count();
            isDeliveryFeeAdded = false;
            

            SetProducts();

            IspickUp = true;
            CheckoutCommand = new Command(async () =>
            {

                var usercards = await CardDataStore.GetCardFromUser(App.LogUser.UserId);

                if (usercards.Count() > 0)
                {

                    if (OrderDetail.OrderStatus == Status.NotSubmited)
                    {
                        if (IsDelivery)
                        {

                            OrderDetail.OrderType = Library.Models.Type.Delivery;
                        }
                        if (IspickUp)
                        {
                            OrderDetail.OrderType = Library.Models.Type.PickUp;
                        }

                        if (!IsDelivery && !IspickUp)
                        {
                            OrderDetail.OrderType = Library.Models.Type.PickUp;
                        }

                        OrderDetail.OrderStatus = Status.Submited;

                        List<PaymentCard> paymentCards = new List<PaymentCard>(usercards);

                        var customercardId = await stripeServiceDS.GetCustomerCardId(App.LogUser.StripeUserId,paymentCards[0].StripeCardId);

                        var isTransactionSuccess = await stripeServiceDS.MakePaymentWithCard(OrderDetail.StoreId, Total, paymentCards[0].PaymentCardId, OrderDetail.OrderId.ToString());
                        if (isTransactionSuccess)
                        {
                            const double fee= 0.02;

                            //var feetransferResult = await stripeServiceDS.TransferQuickOrderFeeFromStore("acct_1HGWePHOvPu5G2cu", fee.ToString(),OrderDetail.StoreId.ToString());
                            var orderUpdate = await orderDataStore.UpdateItemAsync(OrderDetail);

                            if (orderUpdate)
                            {
                                await App.Current.MainPage.DisplayAlert("Notification", "Order was submited...!", "OK");
                            }
                        }
                        else
                        {
                            await App.Current.MainPage.DisplayAlert("Notification", "Bad Transaction.", "OK");
                        }

                       



                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("Notification", "Order was submited.", "OK");
                    }
                }
                else
                {
                    await App.Current.MainPage.DisplayAlert("Notification", "You dont have a card register, register a card first.", "OK");
                }

            });

            CalculateTotal();

            MessagingCenter.Subscribe<ProductPresenter>(this, "RemoveOrderProduct", async (obj) => 
            {
                ProductPresenters.Remove(obj);

                var _orderProductToRemove = OrderDetail.OrderProducts.Where(o => o.OrderProductId == obj.ProductId).FirstOrDefault();

                OrderDetail.OrderProducts.Remove(_orderProductToRemove);

                OrderQuantity = OrderDetail.OrderProducts.Count();

                if (OrderDetail.OrderProducts.Count() == 0)
                {
                    var result = await orderDataStore.DeleteItemAsync(OrderDetail.OrderId.ToString());

                    if (result)
                    {
                        await Shell.Current.GoToAsync($"../StoreOrderRoute");
                    }
                }
                CalculateTotal();


            });

            
        }

     

        public Token stripeToken;
        public TokenService tokenService;
        //public string PublicApiKey = "pk_live_51GOwkJJDC8jrm2WeofpT5zqRgAKZ0LkaQEh64CHvZVcqSvCCBFE7LRV7ZSxjD3pZiTemSKhbe7XBVLX1Q57v2yKc00BW4iat88";
      
       
      

        void SetProducts()
        {
            foreach (var item in OrderDetail.OrderProducts)
            {
                var productPresenter = new ProductPresenter(item);
                ProductPresenters.Add(productPresenter);

            }
        }
        void CalculateTotal()
        {

            if (Total > 0)
            {
                Total = 0;
            }

            if (OrderDetail.OrderProducts.Count() > 0)
            {

                //Total = OrderDetail.OrderProducts.Sum(op => op.Quantity * op.Price);

                //TotalTax = (Total * 0.115);
                //Total = (Total * 0.115) + Total;
                //Total += 0.02;

                //StripeFee = (Total * (2.9 / 100)) + 0.3;
                //Total = (Total * (2.9 / 100)) + Total + 0.30;



                double _netTotal = OrderDetail.OrderProducts.Sum(op => op.Quantity * op.Price);

                Total += _netTotal;

                TotalTax = (Total * 0.115);
                Total += TotalTax;
                Total += 0.02;

                StripeFee = (_netTotal * (2.9 / 100)) + 0.3;
                Total += StripeFee;
            }
            else
            {
                Total = 0.00;
            }


        }
    }
}
