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
	/// Interaction logic for LoadRecordDialog.xaml
	/// </summary>
	public partial class LoadRecordDialog : Window
	{
		public int StepId { get; set; }
		public bool IsStepLoaded { get; set; }
	
		public class StepForShow
		{
			public int Id { get; set; }
			public string Black { get; set; }
			public string White { get; set; }
			public string Result { get; set; }
			public string Description { get; set; }
			public DateTime SaveDate { get; set; }
		}

		public LoadRecordDialog()
		{
			InitializeComponent();
				
			StepId = -1;
			IsStepLoaded = false;
			
			this.grid.DataContext = Steps;
			Loaded += (s, e) => {
				grid.SelectedIndex = 0; 
				grid.Focus();
			};
		}

		public List<StepForShow> Steps
		{
			get
			{
				using (var db = new WeiqiContext()) {
					var r = from s in db.Records
							orderby s.SaveDate descending
							select new StepForShow {
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
			var step = grid.SelectedItem as StepForShow;
			if (step == null) return;
			StepId = step.Id;
			IsStepLoaded = true;
			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			IsStepLoaded = false;
			Close();
		}

		private void grid_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Enter) {
				grid_MouseDoubleClick(sender, null);
			}
		}
	}
}