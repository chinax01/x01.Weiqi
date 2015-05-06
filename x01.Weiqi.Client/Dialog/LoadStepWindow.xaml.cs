// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System.Windows;
using System.Windows.Controls;
using System.Linq;
using x01.Weiqi.Model;
using System.Collections.Generic;
using System;

namespace x01.Weiqi.Dialog
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class LoadStepWindow : Window
	{
		public int StepId { get; set; }
		public bool IsLoadStep { get; set; }
	
		public class StepShow
		{
			public int Id { get; set; }
			public string Black { get; set; }
			public string White { get; set; }
			public string Result { get; set; }
			public string Description { get; set; }
			public DateTime SaveDate { get; set; }
		}

		public LoadStepWindow()
		{
			InitializeComponent();
				
			StepId = -1;
			IsLoadStep = false;
			
			this.grid.DataContext = Steps;
		}

		public List<StepShow> Steps
		{
			get
			{
				using (var db = new WeiqiContext()) {
					var r = from s in db.Steps
							orderby s.SaveDate descending
							select new StepShow {
								Id = s.Id,
								Black = s.Black,
								White = s.White,
								Result = s.Result,
								Description = s.Description,
								SaveDate = s.SaveDate,
							};
					return r.ToList();
				};
			}
		}

	
		private void grid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			StepId = (grid.SelectedItem as StepShow).Id;
			IsLoadStep = true;
			Close();
		}
	}
}