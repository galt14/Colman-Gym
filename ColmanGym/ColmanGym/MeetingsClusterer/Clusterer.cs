using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ColmanGym.Data;
using ColmanGym.Models;
using Microsoft.ML;


namespace ColmanGym.MeetingsClusterer
{
    public class Clusterer
    {
        private readonly ColmanGymContext _context;
        private MLContext _mlContext;
        static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "MeetingsClusteringModel.zip");
        private PredictionEngine<MeetingFeatures, MeetingPrediction> _predictor;
        private readonly int NUM_OF_CLUSTER = 4;

        public Clusterer(ColmanGymContext Context)
        {
            _context = Context;
            _mlContext = new MLContext(seed: 0);
        }

        public static MeetingFeatures ConvertToMeetingFeatures(Meeting meeting)
        {
            return new MeetingFeatures
            {
                TrainingTypeID = meeting.TrainingID.ToString(),
                TrainerID = meeting.TrainerID.ToString(),
                MeetDate = meeting.Date.ToShortDateString(),
                Price = meeting.Price.ToString()
            };
        }

        public void CreateModel()
        {
            IDataView dataView = _mlContext.Data.LoadFromEnumerable<MeetingFeatures>(_context.Meetings.Select(m => ConvertToMeetingFeatures(m)).ToList());
            string featuresColumnName = "Features";
            var pipeline = _mlContext.Transforms.Text.FeaturizeText("TrainingTypeID_F", "TrainingTypeID")
                .Append(_mlContext.Transforms.Text.FeaturizeText("TrainerID_F", "TrainerID"))
                .Append(_mlContext.Transforms.Text.FeaturizeText("MeetDate_F", "MeetDate"))
                .Append(_mlContext.Transforms.Text.FeaturizeText("Price_F", "Price"))
                .Append(_mlContext.Transforms.Concatenate(featuresColumnName, "TrainingTypeID_F", "TrainerID_F", "MeetDate_F", "Price_F"))
                .Append(_mlContext.Clustering.Trainers.KMeans(featuresColumnName, numberOfClusters: NUM_OF_CLUSTER));
            var model = pipeline.Fit(dataView);
            using var fileStream = new FileStream(_modelPath, FileMode.Create, FileAccess.Write, FileShare.Write);
            _mlContext.Model.Save(model, dataView.Schema, fileStream);
            _predictor = _mlContext.Model.CreatePredictionEngine<MeetingFeatures, MeetingPrediction>(model);
        }

        public MeetingPrediction Predict(Meeting meeting)
        {
            MeetingFeatures mf = ConvertToMeetingFeatures(meeting);
            return _predictor.Predict(mf);
        }
    }
}
