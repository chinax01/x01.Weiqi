// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using x01.Weiqi.Model;

namespace x01.Weiqi.Board
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        IStepService m_StepService = new StepService();
        
        public int ID { get; set; }

        public SettingsWindow()
        {
            InitializeComponent();
            int[] stepIDs = m_StepService.GetIDs();
            m_ComboBox.ItemsSource = stepIDs;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ID = (int)m_ComboBox.SelectedItem;
        }

        
    }
}
