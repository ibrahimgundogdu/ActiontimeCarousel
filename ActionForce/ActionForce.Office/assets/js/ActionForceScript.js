function AddResultDocument(id) {

    var typeid = $('#FileTypeID').val();
    var description = $('#FileDescription').val();

    var fotodata = new FormData();

    jQuery.each(jQuery('#ResultFile')[0].files, function (i, file) {
        fotodata.append('file', file);
    });
    fotodata.append('id', id);
    fotodata.append('typeid', typeid);
    fotodata.append('description', description);


    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddResultDocument",
        data: fotodata,

    }).done(function (d) {

        $("#ResultFiles").html(d);


    });
}

function AddCashRecorder(id) {

    var typeid = $('#E13TYPEID').val();
    var description = $('#E13TRLDE').val();
    var slipnumber = $('#E13TRLN').val();
    var slipdate = $('#E13TRLD').val();
    var sliptime = $('#E13TRLT').val();
    var slipamount = $('#E13TRLA').val();
    var slipcashamount = $('#E13TRLNA').val();
    var slipcardamount = $('#E13TRLKA').val();
    var sliptotalmount = $('#E13TRLTA').val();


    var data = new FormData();

    jQuery.each(jQuery('#E13TRLF')[0].files, function (i, file) {
        data.append('file', file);
    });

    data.append('id', id);
    data.append('typeid', typeid);
    data.append('description', description);
    data.append('slipnumber', slipnumber);
    data.append('slipdate', slipdate);
    data.append('sliptime', sliptime);
    data.append('slipcashamount', slipcashamount);
    data.append('slipcardamount', slipcardamount);
    data.append('slipamount', slipamount);
    data.append('sliptotalmount', sliptotalmount);


    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCashRecorder",
        data: data,

    }).done(function (d) {

        $("#CashRecorder").html(d);


    });
}

function AddSalaryEarn(id, itemid, employeeid) {

    var typeid = $('#S8TYPEID' + employeeid).val();
    var description = $('#S8SDE' + employeeid).val();
    var duration = $('#S8SD' + employeeid).val();
    var unithprice = $('#S8SU' + employeeid).val();
    var totalamount = $('#S8ST' + employeeid).val();

    console.log(unithprice);
    console.log(totalamount);
    var data = new FormData();

    //jQuery.each(jQuery('#E13TRLF')[0].files, function (i, file) {
    //    data.append('file', file);
    //});

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('employeeid', employeeid);
    data.append('typeid', typeid);
    data.append('description', description);
    data.append('duration', duration);
    data.append('unithprice', unithprice);
    data.append('totalamount', totalamount);


    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddSalaryEarn",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#EmployeeSalary").html(d);
        ResultSummary(id);

    }).always(function () {
        $("#Loading").hide();
    });
}

function AddSalaryPayment(id, itemid, employeeid) {

    var typeid = $('#S9TYPEID' + employeeid).val();
    var description = $('#S9PDESC' + employeeid).val();
    var amount = $('#S9PAMOUNT' + employeeid).val();


    var data = new FormData();

    //jQuery.each(jQuery('#E13TRLF')[0].files, function (i, file) {
    //    data.append('file', file);
    //});

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('employeeid', employeeid);
    data.append('typeid', typeid);
    data.append('description', description);
    data.append('amount', amount);


    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddSalaryPayment",
        data: data,

    }).done(function (d) {

        $("#EmployeeSalary").html(d);
        ResultSummary(id);

    });
}

function AddBankTransfer(id, itemid) {

    var bankid = $('#BankAccountID' + itemid).val();
    var amount = $('#E7TRLA' + itemid).val();
    var comission = $('#E7TRLCO' + itemid).val();
    var slipnumber = $('#E7TRLN' + itemid).val();
    var slipdate = $('#E7TRLD' + itemid).val();
    var sliptime = $('#E7TRLR' + itemid).val();
    var description = $('#E7TRLDE' + itemid).val();

    var data = new FormData();

    jQuery.each(jQuery('#E7TRLF' + itemid)[0].files, function (i, file) {
        data.append('file', file);
    });

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('bankid', bankid);
    data.append('amount', amount);
    data.append('comission', comission);
    data.append('slipnumber', slipnumber);
    data.append('slipdate', slipdate);
    data.append('sliptime', sliptime);
    data.append('description', description);



    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddBankTransfer",
        data: data,

    }).done(function (d) {

        $("#BankTransfer").html(d);
        ResultSummary(id);

    });
}

function AddExpense(id, itemid) {

    var exptypeid = $('#ExpenseTypeID' + itemid).val();
    var amount = $('#E6TRLA' + itemid).val();
    var currency = $('#E6TRLC' + itemid).val();
    var slipnumber = $('#E6TRLN' + itemid).val();
    var slipdate = $('#SlipDate' + itemid).val();
    var sliptime = $('#SlipTime' + itemid).val();
    var description = $('#E6TRLDE' + itemid).val();

    var data = new FormData();

    jQuery.each(jQuery('#E7TRLF' + itemid)[0].files, function (i, file) {
        data.append('file', file);
    });

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('exptypeid', exptypeid);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('slipnumber', slipnumber);
    data.append('slipdate', slipdate);
    data.append('sliptime', sliptime);
    data.append('description', description);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddExpense",
        data: data,

    }).done(function (d) {
        $("#AddExpense").html(d);
        ResultSummary(id);
    });
}

function AddExchange(id, itemid, currency) {

    var amount = $('#E3A' + currency).val();
    var exchange = $('#E3E' + currency).val();
    var sysamount = $('#E3S' + currency).val();
    var description = $('#E3D' + currency).val();

    var slipnumber = $('#SlipNumber' + currency).val();
    var slipdate = $('#SlipDate' + currency).val();
    var sliptime = $('#SlipTime' + currency).val();

    var data = new FormData();

    jQuery.each(jQuery('#E3F' + currency)[0].files, function (i, file) {
        data.append('file', file);
    });

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('exchange', exchange);
    data.append('sysamount', sysamount);
    data.append('slipnumber', slipnumber);
    data.append('slipdate', slipdate);
    data.append('sliptime', sliptime);
    data.append('description', description);



    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddExchange",
        data: data,

    }).done(function (d) {

        $("#AddExchange").html(d);
        ResultSummary(id);

    });
}

function AddCollect(id, itemid, currency) {

    var amount = $('#T1' + currency).val();
    var description = $('#T1D' + currency).val();

    var data = new FormData();

    data.append('id', id);
    data.append('itemid', itemid);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCollect",
        data: data,

    }).done(function (d) {

        $("#CashCollectPayment").html(d);
        ResultSummary(id);

    });
}

function AddPayment(id, itemid, currency) {

    var amount = $('#T4' + currency).val();
    var description = $('#T4D' + currency).val();

    var data = new FormData();


    data.append('id', id);
    data.append('itemid', itemid);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);



    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddPayment",
        data: data,

    }).done(function (d) {

        $("#CashCollectPayment").html(d);
        ResultSummary(id);

    });
}

function AddCardSale(id, currency) {


    var quantity = $('#S10CSQ').val();
    var amount = $('#S10CSA').val();
    var description = $('#S10CSD').val();

    var data = new FormData();

    data.append('id', id);
    data.append('quantity', quantity);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCardSale",
        data: data,

    }).done(function (d) {

        $("#CardSale").html(d);
        ResultSummary(id);

    });
}

function AddCardRefund(id, currency) {

    var quantity = $('#R11CRQ').val();
    var amount = $('#R11CRA').val();
    var description = $('#R11CRD').val();

    var data = new FormData();

    data.append('id', id);
    data.append('quantity', quantity);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);



    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCardRefund",
        data: data,

    }).done(function (d) {

        $("#CardSale").html(d);
        ResultSummary(id);

    });
}

function AddCashSale(id, currency) {


    var quantity = $('#N2Q' + currency).val();
    var amount = $('#N2A' + currency).val();
    var description = $('#N2D' + currency).val();

    var data = new FormData();

    data.append('id', id);
    data.append('quantity', quantity);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCashSale",
        data: data,

    }).done(function (d) {

        $("#CashSale").html(d);
        ResultSummary(id);

    });
}

function AddCashSaleRefund(id, currency) {

    var quantity = $('#I5Q' + currency).val();
    var amount = $('#I5A' + currency).val();
    var description = $('#I5D' + currency).val();

    var data = new FormData();

    data.append('id', id);
    data.append('quantity', quantity);
    data.append('amount', amount);
    data.append('currency', currency);
    data.append('description', description);



    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/AddCashSaleRefund",
        data: data,

    }).done(function (d) {

        $("#CashSale").html(d);
        ResultSummary(id);

    });
}

function ResultSummary(id) {

    var data = new FormData();

    data.append('id', id);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Result/ResultSummary",
        data: data,

    }).done(function (d) {

        $("#ResultSummary").html(d);


    });
}

function SumCashRecorder() {

    var cash = $("#E13TRLNA").val().replace('.', '').replace(',', '.');
    var card = $("#E13TRLKA").val().replace('.', '').replace(',', '.');
    var toplam = parseFloat(cash) + parseFloat(card) || 0.00;

    $("#E13TRLA").val(toplam.toFixed(2).toString());

    console.log(cash);
    console.log(card);
    console.log(toplam);

}


// shift break scripts

function OpenLocation(id, envid) {

    var result = confirm("Lokasyonu açmak istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/OpenLocation",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (d) {

            var data = d;
            if (typeof (d) == "Object") {
                data = JSON.parse(d);
            }
            console.log(data);
            $("#LocationStatus_" + id + "_2").html(data.Content);


            $.toast({
                heading: 'Information',
                text: data.Message,
                showHideTransition: 'slide',
                icon: 'info',
                hideAfter: 5000,
                position: 'bottom-right',
            });


        }).always(function () {
            $("#Loading").hide();
        });
    };
}

function CloseLocation(id, envid) {

    var result = confirm("Lokasyonu kapatmak istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/CloseLocation",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (d) {

            var data = d;
            if (typeof (d) == "Object") {
                data = JSON.parse(d);
            }
            console.log(data);
            $("#LocationStatus_" + id + "_2").html(data.Content);


            $.toast({
                heading: 'Information',
                text: data.Message,
                showHideTransition: 'slide',
                icon: 'info',
                hideAfter: 5000,
                position: 'bottom-right',
            });


        }).always(function () {
            $("#Loading").hide();
        });
    };
}

function StartEmployeeShift(id, empid, envid) {

    var result = confirm("Çalışanın mesaisini başlatmak istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('employeeid', empid);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(empid);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/StartEmployeeShift",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (data) {
            //EmployeeStatus_175_4477_2
            $("#EmployeeStatus_" + id + "_" + empid + "_2").html(data);
            $("#EmployeeStatus_" + id + "_" + empid + "_2").removeClass();
            $("#EmployeeStatus_" + id + "_" + empid + "_2").addClass('table-info');


        }).always(function () {
            $("#Loading").hide();
        });
    }
}

function FinishEmployeeShift(id, empid, envid) {

    var result = confirm("Çalışanın mesaisini bitirmek istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('employeeid', empid);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(empid);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/FinishEmployeeShift",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (data) {

            $("#EmployeeStatus_" + id + "_" + empid + "_2").html(data);
            $("#EmployeeStatus_" + id + "_" + empid + "_2").removeClass();
            $("#EmployeeStatus_" + id + "_" + empid + "_2").addClass('table-warning');


        }).always(function () {
            $("#Loading").hide();
        });
    }
}

function StartEmployeeBreak(id, empid, envid) {

    var result = confirm("Çalışanın molasını başlatmak istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('employeeid', empid);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(empid);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/StartEmployeeBreak",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (data) {

            $("#EmployeeStatus_" + id + "_" + empid + "_2").html(data);
            $("#EmployeeStatus_" + id + "_" + empid + "_2").removeClass();
            $("#EmployeeStatus_" + id + "_" + empid + "_2").addClass('table-info');


        }).always(function () {
            $("#Loading").hide();
        });
    }
}

function FinishEmployeeBreak(id, empid, envid) {

    var result = confirm("Çalışanın molasını bitirmek istediğinize emin misiniz?");

    if (result) {

        var date = $("#CurrentDateCode").val();
        var data = new FormData();

        data.append('locationid', id);
        data.append('employeeid', empid);
        data.append('environmentid', envid);
        data.append('date', date);

        console.log(id);
        console.log(empid);
        console.log(envid);
        console.log(date);

        $.ajax({
            cache: false,
            contentType: false,
            processData: false,
            method: 'POST',
            type: 'POST',
            url: "/Shift/FinishEmployeeBreak",
            data: data,
            beforeSend: function () {
                $("#Loading").show();
            },
        }).done(function (data) {

            $("#EmployeeStatus_" + id + "_" + empid + "_2").html(data);
            $("#EmployeeStatus_" + id + "_" + empid + "_2").removeClass();
            $("#EmployeeStatus_" + id + "_" + empid + "_2").addClass('table-info');

        }).always(function () {
            $("#Loading").hide();
        });
    }
}

// virman seçim işlemleri

function FLocationCashSelect() {

    var locationid = $('#FromLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/FLocationCashSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectLocationCash").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function FLocationCashSet(id, name) {

    $('#FromCashID').val(id);
    $('#FromName').val(name);

    $('#FromBankID').val('');
    $('#FromEmplID').val('');
    $('#FromCustID').val('');
};

function FLocationBankSelect() {

    var locationid = $('#FromLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/FLocationBankSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectLocationBank").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function FLocationBankSet(id, name) {

    $('#FromBankID').val(id);
    $('#FromName').val(name);

    $('#FromCashID').val('');
    $('#FromEmplID').val('');
    $('#FromCustID').val('');
};

function FLocationEmployeeSelect() {

    var locationid = $('#FromLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/FLocationEmployeeSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectLocationEmployee").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function FLocationEmployeeSet(id, name) {

    $('#FromEmplID').val(id);
    $('#FromName').val(name);

    $('#FromCashID').val('');
    $('#FromBankID').val('');
    $('#FromCustID').val('');
};

function FLocationCustomerSelect() {

    var locationid = $('#FromLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/FLocationCustomerSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectLocationCustomer").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function FLocationCustomerSet(id, name) {

    $('#FromCustID').val(id);
    $('#FromName').val(name);

    $('#FromCashID').val('');
    $('#FromBankID').val('');
    $('#FromEmplID').val('');
};


function TLocationCashSelect() {

    var locationid = $('#ToLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/TLocationCashSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectTLocationCash").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function TLocationCashSet(id, name) {

    $('#ToCashID').val(id);
    $('#ToName').val(name);

    $('#ToBankID').val('');
    $('#ToEmplID').val('');
    $('#ToCustID').val('');
};

function TLocationBankSelect() {

    var locationid = $('#ToLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/TLocationBankSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectTLocationBank").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function TLocationBankSet(id, name) {

    $('#ToBankID').val(id);
    $('#ToName').val(name);

    $('#ToCashID').val('');
    $('#ToEmplID').val('');
    $('#ToCustID').val('');
};

function TLocationEmployeeSelect() {

    var locationid = $('#ToLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/TLocationEmployeeSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectTLocationEmployee").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function TLocationEmployeeSet(id, name) {

    $('#ToEmplID').val(id);
    $('#ToName').val(name);

    $('#ToCashID').val('');
    $('#ToBankID').val('');
    $('#ToCustID').val('');
};

function TLocationCustomerSelect() {

    var locationid = $('#ToLocationID  option:selected').val();
    var date = $('#DocumentDate').val();

    var data = new FormData();

    data.append('locationid', locationid);
    data.append('date', date);

    $.ajax({
        cache: false,
        contentType: false,
        processData: false,
        method: 'POST',
        type: 'POST',
        url: "/Action/TLocationCustomerSelect",
        data: data,
        beforeSend: function () {
            $("#Loading").show();
        },
    }).done(function (d) {

        $("#SelectTLocationCustomer").html(d);

    }).always(function () {
        $("#Loading").hide();
    });

};

function TLocationCustomerSet(id, name) {

    $('#ToCustID').val(id);
    $('#ToName').val(name);

    $('#ToCashID').val('');
    $('#ToBankID').val('');
    $('#ToEmplID').val('');
};

