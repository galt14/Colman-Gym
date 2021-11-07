using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML.Data;


namespace ColmanGym.MeetingsClusterer
{
    public class MeetingFeatures
    {
        [LoadColumn(0)]
        public string TrainingTypeID { get; set; }

        [LoadColumn(1)]
        public string TrainerID { get; set; }

        [LoadColumn(2)]
        public string MeetDate { get; set; }

        [LoadColumn(3)]
        public string Price { get; set; }
    }

    public class MeetingPrediction
    {
        [ColumnName("PredictedLabel")]
        public uint PredictedClusterId;

        [ColumnName("Score")]
        public float[] Distances;
    }
}
