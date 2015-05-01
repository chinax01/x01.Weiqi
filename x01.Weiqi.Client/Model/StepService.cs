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

        public int[] GetIDs()
        {
            using (DbStepEntities entities = new DbStepEntities())
            {
                var result = from s in entities.Steps
                             select s.ID;
                return result.ToArray();
            }
        }

        public string GetSteps(int id)
        {
            using (DbStepEntities entities = new DbStepEntities())
            {
                var result = from s in entities.Steps
                             where s.ID == id
                             select s.StepContent;
                return result.FirstOrDefault();
            }
        }

        public void SaveSteps(string step)
        {
            using (DbStepEntities entities = new DbStepEntities())
            {
                Step s = Step.CreateStep(0, step);
                entities.AddToSteps(s);
                entities.SaveChanges();
                MessageBox.Show("Save success!");
            }
        }

        #endregion
    }
}
