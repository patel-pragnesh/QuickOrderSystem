﻿using QuickOrderApp.ViewModels.StoreAndEmployeesVM;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace QuickOrderApp.Views.Store.EmployeeStoreControlPanel
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StoreControlEmployee : ContentPage
    {
        public StoreControlEmployee()
        {
            InitializeComponent();
            BindingContext = new StoreControlPanelViewModel();
        }

        
    }
}