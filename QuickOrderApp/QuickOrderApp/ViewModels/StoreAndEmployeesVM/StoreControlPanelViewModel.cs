﻿using Library.Models;
using Plugin.Media;
using Plugin.Media.Abstractions;
using QuickOrderApp.Utilities.Static;
using QuickOrderApp.Views.Store.EmployeeStoreControlPanel;
using QuickOrderApp.Views.Store.StoreManger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;

namespace QuickOrderApp.ViewModels.StoreAndEmployeesVM
{
    [QueryProperty("StoreId", "Id")]
    [QueryProperty("EmpStoreId", "EmpStoreId")]
    public class StoreControlPanelViewModel : BaseViewModel
    {

        #region Properties
        private string storeid;

        public string StoreId
        {
            get { return storeid; }
            set
            {
                storeid = value;
                OnPropertyChanged();
                Store = App.LogUser.Stores.Where(s => s.StoreId.ToString() == StoreId).FirstOrDefault();
                YourSelectedStore = Store;
            }
        }

        private string empStoreid;

        public string EmpStoreId
        {
            get { return empStoreid; }
            set
            {
                empStoreid = value;
                OnPropertyChanged();

                GetStoreInformation(EmpStoreId);
                GetWorkHourSchedule();

            }
        }



        byte[] ImgArray;

        private string productname;

        public string ProductName
        {
            get { return productname; }
            set
            {
                productname = value;
                OnPropertyChanged();
            }
        }

        private string productPrice;

        public string ProductPrice
        {
            get { return productPrice; }
            set
            {
                productPrice = value;
                OnPropertyChanged();
            }
        }

        private int productQuantity;

        private string productdescription;

        public string ProductDescription
        {
            get { return productdescription; }
            set
            {
                productdescription = value;
                OnPropertyChanged();
            }
        }


        public int ProductQuantity
        {
            get { return productQuantity; }
            set
            {
                productQuantity = value;
                OnPropertyChanged();
            }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }


        public static Store YourSelectedStore;

        private Store store;

        public Store Store
        {
            get { return store; }
            set
            {
                store = value;
                OnPropertyChanged();
            }
        }

        private Employee storeEmployee;

        public Employee StoreEmployee
        {
            get { return storeEmployee; }
            set
            {
                storeEmployee = value;
                OnPropertyChanged();
            }
        }


        private ImageSource productimg;

        public ImageSource ProductImg
        {
            get { return productimg; }
            set
            {
                productimg = value;
                OnPropertyChanged();
            }
        }


        ObservableCollection<MediaFile> files = new ObservableCollection<MediaFile>();
        private ObservableCollection<EmployeeWorkHour> empWorkHour;

        public ObservableCollection<EmployeeWorkHour> EmpWorkHour
        {
            get { return empWorkHour; }
            set
            {
                empWorkHour = value;
                OnPropertyChanged();
            }
        }

        public List<string> ProductTypes { get; set; }
        private string selectedtype;

        public string SelectedType
        {
            get { return selectedtype; }
            set { selectedtype = value;
                OnPropertyChanged();
            }
        }


        #endregion

        #region Commandos

        public ICommand GoAddProduct { get; set; }
        public ICommand GoInventory { get; set; }
        public ICommand GoOrdersCommand { get; set; }

        public ICommand GoDashboards { get; set; }

        public ICommand GoSearchEmployeeCommand { get; set; }
        public ICommand GoOrdersEmployeeCommand { get; set; }

        public ICommand GoEmployeesCommand { get; set; }
        public ICommand IPickPhotoCommand { get; set; }
        public ICommand CompleteCommand { get; set; }

        public ICommand GoOrderScanner { get; set; }

        #endregion



        public StoreControlPanelViewModel()
        {
            var types = Enum.GetValues(typeof(ProductType));

            ProductTypes = new List<string>();

            foreach (var item in types)
            {
                ProductTypes.Add(item.ToString());
            }


            EmpWorkHour = new ObservableCollection<EmployeeWorkHour>();
            Store = new Store();
            ProductImg = ImageSource.FromFile("imgPlaceholder.jpg");

            GoAddProduct = new Command(async () =>
            {

                await Shell.Current.GoToAsync($"AddProductRoute", animate: true);
            });

            IPickPhotoCommand = new Command(async () =>
            {
                await CrossMedia.Current.Initialize();
                files.Clear();
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await Shell.Current.DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    PhotoSize = PhotoSize.Full,

                });


                if (file == null)
                    return;

                //files.Add(file);

                ImgArray = ConvertToByteArray(file.GetStream());

                ProductImg = ImageSource.FromStream(() => file.GetStream());


            });

            CompleteCommand = new Command(async () =>
            {
                List<string> Values = new List<string>();

                Values.Add(ProductName);
                Values.Add(Description);
                Values.Add(ProductPrice);
                Values.Add(ProductQuantity.ToString());

                if (GlobalValidator.CheckNullOrEmptyPropertiesOfListValues(Values))
                {

                    ProductType _productType = (ProductType)Enum.Parse(typeof(ProductType), SelectedType);

                    var newStoreProduct = new Product()
                    {
                        ProductId = Guid.NewGuid(),
                        InventoryQuantity = ProductQuantity,
                        Price = Convert.ToDouble(ProductPrice),
                        ProductImage = ImgArray,
                        ProductName = ProductName,
                        Type = _productType,
                        StoreId = YourSelectedStore.StoreId,
                        ProductDescription = ProductDescription
                    };

                    var productAddedResult = await productDataStore.AddItemAsync(newStoreProduct);

                    if (productAddedResult)
                    {
                        await Shell.Current.DisplayAlert("Notification", "Product Added", "OK");
                    }
                }
                else
                {
                    await Shell.Current.DisplayAlert("Notification", "Some fields are empty", "OK");
                }
            });

            GoDashboards = new Command(async() => 
            {

                await Shell.Current.DisplayAlert("Notification", "Is not developed yet. Coming Soon.", "OK");
            
            });

            GoOrdersCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"EmployeeOrderControl?Id={YourSelectedStore.StoreId.ToString()}", animate: true);
               

            });

            GoOrdersEmployeeCommand = new Command(async () =>
            {
                await EmployeeShell.Current.GoToAsync($"EmployeeOrderControl?Id={Store.StoreId.ToString()}", animate: true);
              

            });

            GoEmployeesCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"StoreEmployeeRoute?Id={YourSelectedStore.StoreId.ToString()}", animate: true);
                //await Shell.Current.GoToAsync($"OrderPageRoute", animate: true);

            });

            GoInventory = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"InventoryRoute", animate: true);
            });

            GoSearchEmployeeCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync($"{SearchEmployeePage.Route}?id={StoreId}", animate: true);
            });

            GoOrderScanner = new Command(async() =>
            {

                await Shell.Current.GoToAsync($"{OrderScannerPage.Route}");
            });

        }

        byte[] ConvertToByteArray(Stream value)
        {

            byte[] imageArray = null;

            using (MemoryStream memory = new MemoryStream())
            {

                Stream stream = value;
                stream.CopyTo(memory);
                imageArray = memory.ToArray();
            }

            return imageArray;
        }

        async void GetStoreInformation(string value)
        {
            Store = await StoreDataStore.GetItemAsync(value);
        }

        //Obtiene la informacion de horas de trabajos
        async void GetWorkHourSchedule()
        {
            StoreEmployee = await EmployeeDataStore.GetSpecificStoreEmployee(App.LogUser.UserId, Store.StoreId);

            var empWorkHour = await EmployeeWorkHour.GetEmployeeWorkHours(StoreEmployee.EmployeeId.ToString());

            empWorkHour = OrderingEmpWorkHour(empWorkHour.ToList());


            if (empWorkHour.Count() > 0)
            {
                EmpWorkHour.Clear();
                foreach (var item in empWorkHour)
                {

                    if (item != null)
                    {
                        if (item.WillWork)
                        {
                            EmpWorkHour.Add(item);

                        }

                    }
                }

            }
        }

        //Ordena la lista de las hora de trabajo por dias.
        IList<EmployeeWorkHour> OrderingEmpWorkHour(IList<EmployeeWorkHour> employeeWorks)
        {
            EmployeeWorkHour[] orderTempWorkHour = new Library.Models.EmployeeWorkHour[Enum.GetValues(typeof(DayOfWeek)).Length];

            foreach (var item in employeeWorks)
            {
                DayOfWeek day;
                Enum.TryParse(item.Day, out day);

                int valueint;
                if ((int)day == 0)
                {
                    valueint = Enum.GetValues(typeof(DayOfWeek)).Length - 1;
                }
                else
                {
                    valueint = (int)day - 1;

                }
                orderTempWorkHour[valueint] = item;

            }

            return orderTempWorkHour.ToList();
        }

    }
}
