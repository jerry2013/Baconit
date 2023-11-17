using System;
using System.Collections.Generic;
using System.Diagnostics;

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
#if DEBUG
            Debug.WriteLine($"Event [{component.ToString()}] {eventName}");
#endif
        }

        /// <summary>
        /// Reports an event with a string data to the telemetry system.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        public void ReportEvent(object component, string eventName, string data)
        {
        }

        /// <summary>
        /// Reports an event that might need to be looked at, an unexpected event.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="exception"></param>
        public static void ReportUnexpectedEvent(object component, string eventName, Exception exception = null)
        {
#if DEBUG
            Debug.WriteLine($"UnexpectedEvent [{component.ToString()}] {eventName}: {exception}");
#endif
        }

        public static void TrackCrash(Exception exception, IDictionary<string, string> properties)
        {
        }

        /// <summary>
        /// Reports an perf event on how long something took.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="timeTaken"></param>
        public void ReportPerfEvent(object component, string eventName, TimeSpan timeTaken)
        {
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
        }

        /// <summary>
        /// Reports a metric event to the telemetry system.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="eventName"></param>
        /// <param name="metric"></param>
        public void ReportMetric(object component, string eventName, double metric)
        {
        }

        /// <summary>
        /// Track page view
        /// </summary>
        /// <param name="pageName"></param>
        public static void ReportPageView(string pageName)
        {
        }

        /// <summary>
        /// Reports a log event to telemetry.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void ReportLog(object component, string message, string level = null)
        {
        }
    }
}
