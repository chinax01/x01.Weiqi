// --------------------------------------------
// Copyright (c) 2011 by x01(china_x01@qq.com)
// --------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace x01.Weiqi.Model
{
	class StepService : IStepService
	{
		#region IStepService Members

		public int[] GetIds()
		{
			using (var db = new WeiqiContext()) {
				if (db.Steps.Count() == 0) return null;
				var result = from s in db.Steps
							 select s.Id;
				return result.ToArray();
			}
		}

		public string GetContent(int id)
		{
			using (var db = new WeiqiContext()) {
				var result = from s in db.Steps
							 where s.Id == id
							 select s.Content;
				return result.FirstOrDefault();
			}
		}

		public void SaveStep(Step step)
		{
			using (var db = new WeiqiContext()) {
				db.Steps.Add(step);
				db.SaveChanges();
				MessageBox.Show("Save step success!");
			}
		}

		#endregion
	}
}
