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
        this.render();
        // var renderLocations = _.after(locations.length, render);

        // // var renderNotes = _.after(notes.length, render);
        // _.each(notes, function(note) {
        //     note.asyncSave({success: renderNotes});
        // });
    },

    render: function() {
        var _this = this;
        this.collection.each(function(model) {
            _this.$el.append(_this.template(model.toJSON()));
            _this.renderSchedules(model);
        }, this);
    },

    renderSchedules: function(theModel) {
        var _this = this;
        var courseIdToGet = theModel.get('courseId');
        var cityIdToGet = theModel.get('cityId');
        var searchIdToGet = theModel.get('searchId');
        var locationIdToGet = theModel.get('locationId');
        console.log(this.$el);
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
                _this.$el.prev().find('.location-loader').css('display', 'none');
                app.scheduleView = new app.ScheduleView({
                    collection: app.scheduleCollection,
                    el: elemToAppendSchedules,
                    locId: locationIdToGet
                }).render();
            },

            error: function(err) {
                console.log(err);
            }
        });
    },

    showClassLocationMsg: function() {
        this.$el.find('.location-msg').toggleClass('showing');
    }
});