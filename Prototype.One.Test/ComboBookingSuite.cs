﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Prototype.One.Extensions;
using Prototype.One.Test.Data;
using Xunit;

namespace Prototype.One.Test
{
    public class ComboBookingSuite
    {
        [Fact]
        public void create_combo_booking_line_creates_station_added_to_combo_booking_event()
        {
            //
            var description = "COMBO_STATION_DESCRIPTION";
            var stations = new[] { Builder.Station.Build(), Builder.Station.Build() };

            //
            var combo = new ComboBooking(description, stations);

            //
            combo.GetUncommittedEvents()
                    .ShouldAllBeEquivalentTo(stations.Select(s => new StationAddedToComboBookingEvent(s) { AggregateId = combo.Id }));
        }

        [Fact]
        public void change_station_for_combo_booking_creates_station_added_event()
        {
            //
            var initialDescription = "COMBO_STATION_DESCRIPTION";
            var initialStations = new[] { Builder.Station.Build(), Builder.Station.Build() };
            var newDescription = "COMBO_STATION_DESCRIPTION_CHANGED";
            var newStations = new[] { initialStations[0], Builder.Station.Build() };
            var combo = new ComboBooking(initialDescription, initialStations);

            //
            combo.ChangeStations(newDescription, newStations);

            //
            combo.Stations.ShouldAllBeEquivalentTo(newStations);
            combo.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(StationAddedToComboBookingEvent)
                                                            && ((StationAddedToComboBookingEvent)e).AggregateId == combo.Id
                                                            && ((StationAddedToComboBookingEvent)e).Station == newStations[1]);
        }

        [Fact]
        public void change_station_for_combo_booking_creates_station_removed_event()
        {
            //
            var initialDescription = "COMBO_STATION_DESCRIPTION";
            var initialStations = new[] { Builder.Station.Build(), Builder.Station.Build() };
            var newDescription = "COMBO_STATION_DESCRIPTION_CHANGED";
            var newStations = new[] { initialStations[0], Builder.Station.Build() };
            var combo = new ComboBooking(initialDescription, initialStations);

            //
            combo.ChangeStations(newDescription, newStations);

            //
            combo.Stations.ShouldAllBeEquivalentTo(newStations);
            combo.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(StationRemovedFromComboBookingEvent)
                                                            && ((StationRemovedFromComboBookingEvent)e).AggregateId == combo.Id
                                                            && ((StationRemovedFromComboBookingEvent)e).Station == initialStations[1]);
        }
    }

    public class ComboBooking : Aggregate
    {
        public ComboBooking(string stationDescription, IEnumerable<StationId> stations)
            : base()
        {
            _stations = new List<StationId>(stations.Count());

            _stationDescription = stationDescription;
            SetStations(stations);
        }

        string _stationDescription;
        List<StationId> _stations;
        public IEnumerable<StationId> Stations { get { return _stations; } }

        public void ChangeStations(string newDescription, IEnumerable<StationId> newStations)
        {
            _stationDescription = newDescription;
            SetStations(newStations);
        }

        void SetStations(IEnumerable<StationId> stations)
        {
            foreach (var station in stations.Where(s => _stations.DoesNotContain(s)))
            {
                _stations.Add(station);
                this.RaiseEvent(new StationAddedToComboBookingEvent(station));
            }

            foreach (var station in _stations.Where(s => stations.DoesNotContain(s))
                                                .ToList())
            {
                _stations.Remove(station);
                this.RaiseEvent(new StationRemovedFromComboBookingEvent(station));
            }
        }
    }

    public class StationAddedToComboBookingEvent : DomainEvent
    {
        public StationAddedToComboBookingEvent(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }

    public class StationRemovedFromComboBookingEvent : DomainEvent
    {
        public StationRemovedFromComboBookingEvent(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }
}
