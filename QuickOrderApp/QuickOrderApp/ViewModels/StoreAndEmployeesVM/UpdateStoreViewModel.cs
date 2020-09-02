﻿using Library.Models;
using Library.SolutionUtilities.ValidatorComponents;
using Plugin.Media;
using Plugin.Media.Abstractions;
using QuickOrderApp.Utilities.Presenters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace QuickOrderApp.ViewModels.StoreAndEmployeesVM
{
    public class UpdateStoreViewModel:BaseViewModel
    {

        public UpdateStoreViewModel()
        {

            StoreNameValidator = new Validator();
            SelectedTypeValidator = new Validator();
            StoreDescriptionValidator = new Validator();

            StoreImage = ImageSource.FromFile("imgPlaceholder.jpg");
            StoreTypes = new List<string>(Enum.GetNames(typeof(StoreType)).ToList());

            #region WorkHourPresenter Initialize
            WorkHourPresenters = new ObservableCollection<WorkHourPresenter>();

            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Monday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Tuesday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Wednesday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Thursday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Friday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Saturday.ToString()));
            WorkHourPresenters.Add(new WorkHourPresenter(DayOfWeek.Sunday.ToString()));
            #endregion


            MessagingCenter.Subscribe<Store>(this, "StoreUpdateMsg", (sender) =>
            {
                StoreUpdate = sender;
               
            });
        }


        private Store storeUpdate;

        public Store StoreUpdate
        {
            get { return storeUpdate; }
            set { storeUpdate = value; }
        }


        private Validator storeNameValidator;

        public Validator StoreNameValidator
        {
            get { return storeNameValidator; }
            set
            {
                storeNameValidator = value;
                OnPropertyChanged();
            }
        }

        private Validator selectedTypeValidator;

        public Validator SelectedTypeValidator
        {
            get { return selectedTypeValidator; }
            set
            {
                selectedTypeValidator = value;
                OnPropertyChanged();
            }
        }

        private Validator storeDescriptionValidator;

        public Validator StoreDescriptionValidator
        {
            get { return storeDescriptionValidator; }
            set { storeDescriptionValidator = value;
                OnPropertyChanged();
            }
        }


        private string storeTypeSelected;

        public string StoreTypeSelected
        {
            get { return storeTypeSelected; }
            set
            {
                storeTypeSelected = value;
                OnPropertyChanged();
            }
        }

        private string storeDescription;

        public string StoreDescription
        {
            get { return storeDescription; }
            set { storeDescription = value;
                OnPropertyChanged();
            }
        }


        private ImageSource storeImage;

        public ImageSource StoreImage
        {
            get { return storeImage; }
            set
            {
                storeImage = value;
                OnPropertyChanged();
            }
        }

        private string storename;

        public string StoreName
        {
            get { return storename; }
            set
            {
                if (storename != value)
                {

                    storename = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> StoreTypes { get; set; }

        byte[] ImgArray;

        public ObservableCollection<WorkHourPresenter> WorkHourPresenters { get; set; }
        ObservableCollection<MediaFile> files = new ObservableCollection<MediaFile>();
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

        IList<WorkHour> GetStoreWorkHours(IList<WorkHourPresenter> workHourPresenters)
        {
            List<WorkHour> workHours = new List<WorkHour>();

            foreach (var item in WorkHourPresenters)
            {
                var workHour = new WorkHour()
                {
                    CloseTime = Convert.ToDateTime(item.Close.ToString()),
                    Day = item.Day,
                    OpenTime = Convert.ToDateTime(item.Open.ToString()),
                    WorkHourId = Guid.NewGuid(),
                    StoreId = StoreUpdate.StoreId
                };

                workHours.Add(workHour);
            }

            return workHours;
        }

        public ICommand PickPhotoCommand => new Command(async() => 
        {

            try
            {

                await CrossMedia.Current.Initialize();

                files.Clear();

                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await Shell.Current.DisplayAlert("Photos Not Supported", ":( Permission not granted to photos.", "OK");
                    return;
                }

                var mediOptions = new PickMediaOptions()
                {
                    CompressionQuality=99,
                     

                };

                var selectedImgFile = await CrossMedia.Current.PickPhotoAsync(mediOptions);



                if (selectedImgFile == null)
                    return;



                ImgArray = ConvertToByteArray(selectedImgFile.GetStream());

                StoreImage = ImageSource.FromStream(() => selectedImgFile.GetStream());
            }
            catch (Exception e)
            {

                Console.WriteLine(e);
            }

        });

        public ICommand UpdateCompleted => new Command(async () => 
        {

            StoreNameValidator = ValidatorRules.EmptyOrNullValueRule(StoreName);
            SelectedTypeValidator = ValidatorRules.EmptyOrNullValueRule(StoreTypeSelected);
            StoreDescriptionValidator = ValidatorRules.EmptyOrNullValueRule(StoreDescription);

            if (!StoreNameValidator.HasError && !SelectedTypeValidator.HasError && !StoreDescriptionValidator.HasError)
            {

                StoreUpdate.StoreName = StoreName;

                Library.Models.StoreType type;

                Enum.TryParse(StoreTypeSelected, out type);

                StoreUpdate.StoreType = type;
                StoreUpdate.StoreDescription = StoreDescription;

                StoreUpdate.WorkHours = GetStoreWorkHours(WorkHourPresenters);

                if (ImgArray != null )
                {
                    StoreUpdate.StoreImage = ImgArray;
                }

                var storeResult =await StoreDataStore.UpdateItemAsync(StoreUpdate);

                if (storeResult)
                {
                    await Shell.Current.DisplayAlert("Notification", "Update", "OK");

                    MessagingCenter.Send<Store>(StoreUpdate, "UpdatedStore");
                }


            }


        });
    }
}
