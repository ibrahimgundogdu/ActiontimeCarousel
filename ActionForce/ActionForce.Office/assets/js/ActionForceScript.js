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

    }).done(function (d) {

        $("#EmployeeSalary").html(d);


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


    });
}

function AddBankTransfer(id, itemid) {

    var bankid = $('#BankAccountID' + itemid).val();
    var amount = $('#E7TRLA' + itemid).val();
    var comission = $('#E7TRLCO' + itemid).val();
    var slipnumber = $('#E7TRLN' + itemid).val();
    var slipdate = $('#E7TRLD' + itemid).val();
    var sliptime = $('#E7TRLD' + itemid).val();
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


    });
}