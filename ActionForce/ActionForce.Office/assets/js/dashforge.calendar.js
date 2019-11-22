
$(function () {
    'use strict'

    // Initialize tooltip
    $('[data-toggle="tooltip"]').tooltip()

    // Sidebar calendar
    $('#calendarInline').datepicker({
        showOtherMonths: true,
        selectOtherMonths: true,
        beforeShowDay: function (date) {

            // add leading zero to single digit date
            var day = date.getDate();
            console.log(day);
            return [true, (day < 10 ? 'zero' : '')];
        },
        dayNamesMin: ["Pz", "Pt", "Sa", "Ca", "Pe", "Cu", "Ct"],
        monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
        firstDay: 1
    });


    // Initialize fullCalendar
    $('#calendar').fullCalendar({
        height: 'parent',
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,agendaWeek,agendaDay,listWeek'
        },
        locale: 'tr',
        dayNames: ["Pazar", "Pazartesi", "Salı", "Çarsamba", "Perşembe", "Cuma", "Cumartesi"],
        dayNamesShort: ['Pazar', 'Pazartesi', 'Salı', 'Çarşamba', 'Perşembe', 'Cuma', 'Cumartesi'],
        monthNames: ["Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık"],
        firstDay: 1,
        navLinks: true,
        selectable: true,
        selectLongPressDelay: 100,
        editable: true,
        nowIndicator: true,
        eventStartEditable: false,
        defaultView: 'listMonth',
        views: {
            agenda: {
                columnHeaderHtml: function (mom) {
                    return '<span>' + mom.format('ddd') + '</span>' +
                        '<span>' + mom.format('DD') + '</span>';
                }
            },
            day: { columnHeader: false },
            listMonth: {
                listDayFormat: 'ddd DD',
                listDayAltFormat: false
            },
            listWeek: {
                listDayFormat: 'ddd DD',
                listDayAltFormat: false
            },
            agendaThreeDay: {
                type: 'agenda',
                duration: { days: 3 },
                titleFormat: 'MMMM YYYY'
            }
        },
        code: "tr",
        week: {
            dow: 1,
            doy: 7 // The week that contains Jan 1st is the first week of the year.
        },
        buttonText: {
            prev: " Geri ",
            next: " İleri ",
            today: "Bugün",
            month: "Ay",
            week: "Hafta",
            day: "Gün",
            list: "Ajanda"
        },
        weekLabel: "Hf",
        allDayText: "Tüm gün",
        eventLimitText: "Daha fazla",
        noEventsMessage: "Gösterilecek etkinlik yok",
        

    eventSources: [calendarEvents, birthdayEvents, holidayEvents, discoveredEvents, meetupEvents, otherEvents],
        eventAfterAllRender: function (view) {
            if (view.name === 'listMonth' || view.name === 'listWeek') {
                var dates = view.el.find('.fc-list-heading-main');
                dates.each(function () {
                    var text = $(this).text().split(' ');
                    var now = moment().format('DD');

                    $(this).html(text[0] + '<span>' + text[1] + '</span>');
                    if (now === text[1]) { $(this).addClass('now'); }
                });
            }

            console.log(view.el);
        },
        eventRender: function (event, element) {

            if (event.description) {
                element.find('.fc-list-item-title').append('<span class="fc-desc">' + event.description + '</span>');
                element.find('.fc-content').append('<span class="fc-desc">' + event.description + '</span>');
            }

            var eBorderColor = (event.source.borderColor) ? event.source.borderColor : event.borderColor;
            element.find('.fc-list-item-time').css({
                color: eBorderColor,
                borderColor: eBorderColor
            });

            element.find('.fc-list-item-title').css({
                borderColor: eBorderColor
            });

            element.css('borderLeftColor', eBorderColor);
        },
    });

    var calendar = $('#calendar').fullCalendar('getCalendar');

    // change view to week when in tablet
    if (window.matchMedia('(min-width: 576px)').matches) {
        calendar.changeView('agendaWeek');
    }

    // change view to month when in desktop
    if (window.matchMedia('(min-width: 992px)').matches) {
        calendar.changeView('month');
    }

    // change view based in viewport width when resize is detected
    calendar.option('windowResize', function (view) {
        if (view.name === 'listWeek') {
            if (window.matchMedia('(min-width: 992px)').matches) {
                calendar.changeView('month');
            } else {
                calendar.changeView('listWeek');
            }
        }
    });

    // Display calendar event modal
    calendar.on('eventClick', function (calEvent, jsEvent, view) {

        var modal = $('#modalCalendarEvent');

        modal.modal('show');
        modal.find('.event-title').text(calEvent.title);

        if (calEvent.description) {
            modal.find('.event-desc').text(calEvent.description);
            modal.find('.event-desc').prev().removeClass('d-none');
        } else {
            modal.find('.event-desc').text('');
            modal.find('.event-desc').prev().addClass('d-none');
        }

        modal.find('.event-start-date').text(moment(calEvent.start).format('LLL'));
        modal.find('.event-end-date').text(moment(calEvent.end).format('LLL'));

        //styling
        modal.find('.modal-header').css('backgroundColor', (calEvent.source.borderColor) ? calEvent.source.borderColor : calEvent.borderColor);
    });

    // display current date
    var dateNow = calendar.getDate();
    calendar.option('select', function (startDate, endDate) {
        $('#modalCreateEvent').modal('show');
        $('#eventStartDate').val(startDate.format('LL'));
        $('#eventEndDate').val(endDate.format('LL'));

        $('#eventStartTime').val(startDate.format('LT')).trigger('change');
        $('#eventEndTime').val(endDate.format('LT')).trigger('change');
    });

    $('.select2-modal').select2({
        minimumResultsForSearch: Infinity,
        dropdownCssClass: 'select2-dropdown-modal',
    });

    $('.calendar-add').on('click', function (e) {
        e.preventDefault()

        $('#modalCreateEvent').modal('show');
    });

})
