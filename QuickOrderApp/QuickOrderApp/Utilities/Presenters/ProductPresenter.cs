﻿using Library.Models;
using QuickOrderApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace QuickOrderApp.Utilities.Presenters
{
    public class ProductPresenter:BaseViewModel
    {

		private Guid productid;

		public Guid ProductId
		{
			get { return productid; }
			set { productid = value;
				OnPropertyChanged();
			}
		}

		private string productName;

		public string ProductName
		{
			get { return productName; }
			set { productName = value;
				OnPropertyChanged();
			}
		}

		private double productprice;

		public double ProductPrice
		{
			get { return productprice; }
			set { productprice = value;
				OnPropertyChanged();
			}
		}

		private byte[] img;

		public byte[] ProductImg
		{
			get { return img; }
			set
			{
				img = value;
				OnPropertyChanged();
			}
		}

		private double quantity;

		public double Quantity
		{
			get { return quantity; }
			set { quantity = value;
				OnPropertyChanged();
			}
		}

		public ICommand AddToCartCommand { get; set; }

		public ProductPresenter(Product product)
		{
			ProductName = product.ProductName;
			ProductImg = product.ProductImage;
			ProductId = product.ProductId;
			ProductPrice = product.Price;

			Quantity = 0;

			AddToCartCommand = new Command(async () => 
			{

                if (Quantity > 0)
                {

					var userhaveorder =  orderDataStore.HaveOrder(App.LogUser.UserId, App.CurrentStore.StoreId);

					if (userhaveorder == null)
					{
						Order order = new Order()
						{
							OrderId = Guid.NewGuid(),
							BuyerId = App.LogUser.UserId,
							StoreId = App.CurrentStore.StoreId,
							OrderType = Library.Models.Type.None,
							OrderDate = DateTime.Today,
							OrderStatus = Status.NotSubmited,
						

						};

						var orderadded = await orderDataStore.AddItemAsync(order);

						OrderProduct orderProduct = new OrderProduct()
						{
							OrderProductId = Guid.NewGuid(),
							Price = ProductPrice,
							ProductName = ProductName,
							Quantity = (int)Quantity,
							BuyerId = App.LogUser.UserId,
							StoreId = App.CurrentStore.StoreId,
							OrderId = order.OrderId,
							ProductImage = ProductImg
						};

						var result = orderProductDataStore.OrderProductOfUserExist(App.LogUser.UserId, orderProduct.ProductName);
						if (result == false)
						{

							var orderProductAdded = await orderProductDataStore.AddItemAsync(orderProduct);
						}
						else
						{

							var toupdate = orderProductDataStore.OrderProductOfUserExistWith(ProductName);

							if (toupdate.Quantity != orderProduct.Quantity)
							{

								toupdate.Quantity = orderProduct.Quantity;
								var uptedResult = await orderProductDataStore.UpdateItemAsync(toupdate);
							}
						}

					}
					else
					{

						OrderProduct orderProduct = new OrderProduct()
						{
							OrderProductId = Guid.NewGuid(),
							Price = ProductPrice,
							ProductName = ProductName,
							Quantity = (int)Quantity,
							BuyerId = App.LogUser.UserId,
							StoreId = App.CurrentStore.StoreId,
							 OrderId = userhaveorder.OrderId,
							ProductImage = ProductImg
						};

						var result = orderProductDataStore.OrderProductOfUserExist(App.LogUser.UserId, orderProduct.ProductName);
						if (result == false)
						{

							var orderProductAdded = await orderProductDataStore.AddItemAsync(orderProduct);
						}
						else
						{

							var toupdate = orderProductDataStore.OrderProductOfUserExistWith(ProductName);

							if (toupdate.Quantity != orderProduct.Quantity)
							{

								toupdate.Quantity = orderProduct.Quantity;
								var uptedResult = await orderProductDataStore.UpdateItemAsync(toupdate);
							}
						}
					}

                   
                }




            });

		}




	}
}
