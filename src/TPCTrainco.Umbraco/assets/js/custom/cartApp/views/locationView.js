'use strict';

window.app = window.app || {};

app.LocationView = Backbone.View.extend({
    tagName: 'div',
    className: 'one-location',

    events: {
        'click .location-icon': 'showClassLocationMsg'
    },

    template: _.template($('#locationTemplate').html()),

    initialize: function() {
        // this.render();
        var _this = this;
        setTimeout(function() {
            _this.render()
        }, 2);

        // the counter, which enables us to wait until last schedules ajax call
        this.fetchCounter = this.collection.length;
    },

    render: function() {
        var _this = this;
        this.locationIdArr = [];
        this.collection.each(function(model) {
            var locationIdToGet = model.get('locationId');
            this.locationIdArr.push(locationIdToGet);
            _this.$el.append(_this.template(model.toJSON()));
            setTimeout(function() {
                _this.renderSchedules(model);
            }, 1);
            
        }, this);
    },

    renderSchedules: function(theModel) {
        var _this = this;
        var courseIdToGet = theModel.get('courseId');
        var cityIdToGet = theModel.get('cityId');
        var searchIdToGet = theModel.get('searchId');
        var locationIdToGet = theModel.get('locationId');
        var elemToAppendSchedules = this.$('.schedule-items-wrap');
        this.$el.prev().find('.location-loader').css('display', 'block');
        
        app.scheduleCollection.fetch({
            remove: false,
            data: JSON.stringify({
                "courseId": courseIdToGet,
                "cityId": cityIdToGet,
                "searchId": searchIdToGet,
                "locationId": locationIdToGet
            }),
            type: "POST",
            contentType: "application/json",

            success: function(data) {
                _this.fetchCounter -= 1;
                if (_this.fetchCounter === 0) {
                    console.log('fetching schedule')
                    _this.$el.prev().find('.location-loader').css('display', 'none');
                    app.scheduleView = new app.ScheduleView({
                        collection: app.scheduleCollection,
                        el: elemToAppendSchedules,
                        locationLocId: _this.locationIdArr
                    });
                }
            },

            error: function(err) {
                console.log(err);
            }
        });
    },

    showClassLocationMsg: function(e) {
        e.preventDefault();
        var target = $(e.currentTarget);
        target.parent().parent().find('.location-msg').toggleClass('showing');
    }
});