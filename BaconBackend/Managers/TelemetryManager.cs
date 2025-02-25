﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;

namespace BaconBackend.Managers
{
    public class TelemetryManager
    {    
        private static BaconManager _baconMan;

        public TelemetryManager(BaconManager baconMan)
        {
            _baconMan = baconMan;
        }

        /// <summary>
        /// Reports an event to the telemetry manager.
        /// </summary>
        /// <param name="component">The object reporting the event, this will be logged</param>
        /// <param name="eventName"></param>
        public static void ReportEvent(object component, string eventName)
        {
            TelemetryClientInstance?.TrackEvent(component.GetType().Name + ":" +eventName);
        }

        /// <summary>
        /// Reports an event with a string data to the telemetry system.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        public void ReportEvent(object component, string eventName, string data)
        {
            var eventT = new EventTelemetry {Name = component.GetType().Name + ":" + eventName};
            eventT.Properties.Add("data", data);
            TelemetryClientInstance?.TrackEvent(eventName);
        }

        /// <summary>
        /// Reports an event that might need to be looked at, an unexpected event.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="exception"></param>
        public static void ReportUnexpectedEvent(object component, string eventName, Exception exception = null)
        {
            var eventT = new EventTelemetry {Name = component.GetType().Name + ":" + eventName};
            eventT.Properties.Add("error", "unexpected");
            if(exception != null)
            {
                eventT.Properties.Add("exception", exception.Message);
            }
            TelemetryClientInstance?.TrackEvent(eventName);
        }

        public static void TrackCrash(Exception exception, IDictionary<string, string> properties)
        {
            TelemetryClientInstance?.TrackException(exception, properties);
        }

        /// <summary>
        /// Reports an perf event on how long something took.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="timeTaken"></param>
        public void ReportPerfEvent(object component, string eventName, TimeSpan timeTaken)
        {
            TelemetryClientInstance?.TrackMetric(component.GetType().Name + ":" + eventName, timeTaken.TotalMilliseconds);
        }

        /// <summary>
        /// Reports an perf event on how long something took. Here you pass the begin
        /// time and the delta will be computed
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="startTime"></param>
        public static void ReportPerfEvent(object component, string eventName, DateTime startTime)
        {
            TelemetryClientInstance?.TrackMetric(component.GetType().Name + ":" + eventName, (DateTime.Now - startTime).TotalMilliseconds);
        }

        /// <summary>
        /// Reports a metric event to the telemetry system.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="metric"></param>
        public void ReportMetric(object component, string eventName, double metric)
        {
            TelemetryClientInstance?.TrackMetric(component.GetType().Name + ":" + eventName + ":" + eventName, metric);
        }

        /// <summary>
        /// Track page view
        /// </summary>
        /// <param name="pageName"></param>
        public static void ReportPageView(string pageName)
        {
            TelemetryClientInstance?.TrackPageView(pageName);
        }

        /// <summary>
        /// Reports a log event to telemetry.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void ReportLog(object component, string message, SeverityLevel level = SeverityLevel.Information)
        {
            TelemetryClientInstance?.TrackTrace($"[{component.GetType().Name}] {message}", level);
        }

        private static TelemetryClient _telemetryClient;
        private static TelemetryClient TelemetryClientInstance
        {
            get
            {
                if (_baconMan.UiSettingsMan.AnalyticCollection && _telemetryClient == null)
                {
                    _telemetryClient = new TelemetryClient();
                }

                if (_baconMan.UiSettingsMan.AnalyticCollection == false)
                {
                    _telemetryClient = null;
                }

                return _telemetryClient;
            }
        }
    }
}
