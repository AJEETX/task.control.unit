(g => { var h, a, k, p = "API", c = "google", l = "importLibrary", q = "__ib__", m = document, b = window; b = b[c] || (b[c] = {}); var d = b.maps || (b.maps = {}), r = new Set, e = new URLSearchParams, u = () => h || (h = new Promise(async (f, n) => { await (a = m.createElement("script")); e.set("libraries", [...r] + ""); for (k in g) e.set(k.replace(/[A-Z]/g, t => "_" + t[0].toLowerCase()), g[k]); e.set("callback", c + ".maps." + q); a.src = `https://maps.${c}apis.com/maps/api/js?` + e; d[q] = f; a.onerror = () => h = n(Error(p + " could not load.")); a.nonce = m.querySelector("script[nonce]")?.nonce || ""; m.head.append(a) })); d[l] ? console.warn(p + " only loads once. Ignoring:", g) : d[l] = (f, ...n) => r.add(f) && u().then(() => d[l](f, ...n)) })
    ({ key: "AIzaSyDH8T9FvJ8n2LNwxkppRAeOq3Mx7I3qi1E", v: "beta" });

let mapz;
var showCustomerMap = false;
var showBeneficiaryMap = false;
var showFaceMap = false;
var showLocationMap = false;
var showOcrMap = false;
const image =
    "https://developers.google.com/maps/documentation/javascript/examples/full/images/beachflag.png";
$(document).ready(function () {
    $('#datatable thead th').css('background-color', '#e9ecef')
    var datatable = $('#datatable').dataTable({
        processing: true,
        ordering: true,
        paging: true,
        searching: true,
        //'fnDrawCallback': function (oSettings) {
        //    $('.dataTables_filter').each(function () {
        //        $(this).prepend('<button class="btn btn-success mr-xs pull-right" type="button"><i class="fas fa-plus"></i>  Add</button>');
        //    });
        //}
    });

    $("#datepicker").datepicker({ maxDate: '0' });

    if ($(".selected-case:checked").length) {
        $("#allocate-case").prop('disabled', false);
    }
    else {
        $("#allocate-case").prop('disabled', true);
    }

    // When user checks a radio button, Enable submit button
    $(".selected-case").change(function (e) {
        if ($(this).is(":checked")) {
            $("#allocate-case").prop('disabled', false);
        }
        else {
            $("#allocate-case").prop('disabled', true);
        }
    });

    $('#RawMessage').summernote({
        height: 300,                 // set editor height
        minHeight: null,             // set minimum height of editor
        maxHeight: null,             // set maximum height of editor
        focus: true                  // set focus to editable area after initializing summernote
    });

    $("#receipient-email").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/MasterData/GetUserBySearch",
                type: "POST",
                data: { search: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item, value: item };
                    }))
                }
            })
        },
        messages: {
            noResults: "",
            results: function (r) {
                return r;
            }
        },
        minLength: 3
    });

    $('.row-links').on('click', function () {
        let form = $('#message-detail');
        form.submit();
    });

    $('tbody tr').on('click', function () {
        let id = $(this).data('url');
        if (typeof id !== 'undefined') {
            window.location.href = id;
        }
    });

    // delete messages
    $('#delete-messages').on('click', function () {
        let ids = [];
        let form = $('#listForm');
        let checkboxArray = document.getElementsByName('ids');

        // check if checkbox is checked
        for (let i = 0; i < checkboxArray.length; i++) {
            if (checkboxArray[i].checked)
                ids.push(checkboxArray[i].value);
        }

        // submit form
        if (ids.length > 0) {
            if (confirm("Are you sure you want to delete this item(s)?")) {
                form.submit();
            }
        }
    });

    $('#delete-message').on('click', function () {
        $('#deleteForm').submit();
    });

    // Attach the call to toggleChecked to the
    // click event of the global checkbox:
    $("#checkall").click(function () {
        var status = $("#checkall").prop('checked');
        $('#manage-vendors').prop('disabled', !status)
        toggleChecked(status);
    });

    $("input.vendors").click(function () {
        var checkboxes = $("input[type='checkbox'].vendors");
        var anyChecked = checkIfAnyChecked(checkboxes);
        var allChecked = checkIfAllChecked(checkboxes);
        $('#checkall').prop('checked', allChecked);
        $('#manage-vendors').prop('disabled', !anyChecked)
    });

    $("#btnDeleteImage").click(function () {
        var id = $(this).attr("data-id");
        $.ajax({
            url: '/User/DeleteImage/' + id,
            type: "POST",
            async: true,
            success: function (data) {
                if (data.succeeded) {
                    $("#delete-image-main").hide();
                    $("#ProfilePictureUrl").val("");
                }
                else {
                    // toastr.error(data.message);
                }
            },
            beforeSend: function () {
                $(this).attr("disabled", true);
                $(this).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Loading...');
            },
            complete: function () {
                $(this).html('Delete Image');
            },
        });
    });

    $('.face-Image').click(function () {
        var data;
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetInvestigationData?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    data = response;
                    self.setTitle('<i class="fas fa-portrait"></i> Digital <span class="badge badge-light">checkify</span>');
                    self.setContent('<span class="badge badge-light"><i class="far fa-image"></i> Digital Image</span>');
                    self.setContentAppend('<br><img id="agentLocationPicture" class="img-fluid investigation-actual-image" src="' + response.location + '" /> ');
                    self.setContentAppend('<br><span class="badge badge-light"><i class="fas fa-info"></i> Location Info</span> ');
                    self.setContentAppend('<br><i>' + response.locationData + '</i>');
                    showFaceMap = true;
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            },
            onContentReady: function () {
                if (showFaceMap) {
                    showFaceMap = false;
                    initPopMap(data.position, data.address);
                }
            }
        })
    })

    $('.locationImage').click(function () {
        var data;
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetInvestigationData?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    data = response;
                    self.setTitle('<i class="fas fa-portrait"></i> Digital <span class="badge badge-light">checkify</span>');
                    self.setContent('<span class="badge badge-light"><i class="fas fa-map-pin"></i> Location visited</span>:');
                    self.setContentAppend('<div id="maps"></div>')
                    self.setContentAppend('<br><div id="pop-face-map"></div>')
                    self.setContentAppend('</div>')
                    self.setContentAppend('<span class="badge badge-light"><i class="fas fa-map-marker-alt"></i> Address visited</span>:');
                    self.setContentAppend('<br><i>' + response.imageAddress + '</i>');
                    showLocationMap = true;
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            },
            onContentReady: function () {
                if (showLocationMap) {
                    showLocationMap = false;
                    initPopMap(data.facePosition, data.imageAddress);
                }
            }
        })
    })

    $('.olocationImage').click(function () {
        var data;
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetInvestigationData?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    data = response;
                    self.setTitle('<i class="fas fa-portrait"></i> Document <span class="badge badge-light">checkify</span>');
                    self.setContent('<span class="badge badge-light"><i class="fas fa-map-pin"></i> Location visited</span>:');
                    self.setContentAppend('<div id="maps"></div>')
                    self.setContentAppend('<br><div id="pop-face-map"></div>')
                    self.setContentAppend('</div>')
                    self.setContentAppend('<span class="badge badge-light"><i class="fas fa-map-marker-alt"></i> Address visited</span>:');
                    self.setContentAppend('<br><i>' + response.imageAddress + '</i>');
                    showOcrMap = true;
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            },
            onContentReady: function () {
                if (showOcrMap) {
                    showOcrMap = false;
                    initPopMap(data.ocrPosition, data.ocrAddress);
                }
            }
        })
    })

    $('#profileImageMap').click(function () {
        var data;
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetCustomerMap?id=' + $('#customerDetailId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    data = response;
                    self.setTitle('<i class="fas fa-mobile-alt"></i> <b>Customer Address Location</b>');
                    self.setContent('<b><span class="badge badge-light"><i class="fas fa-map-pin"></i> Map Location</span></b>:');
                    self.setContentAppend('<div id="maps"></div>')
                    self.setContentAppend('<br><div id="pop-face-map"></div>')
                    self.setContentAppend('</div>')
                    self.setContentAppend('<span class="badge badge-light"><i class="fas fa-map-marker-alt"></i> Address</span>:');
                    self.setContentAppend('<br><i>' + response.address + '</i>');
                    self.setContentAppend('<br><span class="badge badge-light"><i class="fas fa-info"></i> Location Info</span> :');
                    self.setContentAppend('<br><i>' + response.weatherData + '</i>');
                    showCustomerMap = true;
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            },
            onContentReady: function () {
                if (showCustomerMap) {
                    showCustomerMap = false;
                    initPopMap(data.position, data.address);
                }
            }
        })
    });

    $('#bImageMap').click(function () {
        var data;
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetBeneficiaryMap?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    data = response;
                    self.setTitle('<i class="fas fa-mobile-alt"></i> <b>Beneficiary Address Location</b>');
                    self.setContent('<b><span class="badge badge-light"><i class="fas fa-map-pin"></i> Map Location</span></b>:');
                    self.setContentAppend('<div id="maps"></div>')
                    self.setContentAppend('<br><div id="pop-face-map"></div>')
                    self.setContentAppend('</div>')
                    self.setContentAppend('<span class="badge badge-light"><i class="fas fa-map-marker-alt"></i> Address</span>:');
                    self.setContentAppend('<br><i>' + response.address + '</i>');
                    self.setContentAppend('<br><span class="badge badge-light"><i class="fas fa-info"></i> Location Info</span> :');
                    self.setContentAppend('<br><i>' + response.weatherData + '</i>');
                    showBeneficiaryMap = true;
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            },
            onContentReady: function () {
                if (showBeneficiaryMap) {
                    showBeneficiaryMap = false;
                    initPopMap(data.position, data.address);
                }
            }
        })
    })

    $('.ocr-Image').click(function () {
        $.confirm({
            type: 'green',
            closeIcon: true,
            columnClass: 'medium',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetInvestigationData?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    self.setTitle('<i class="fas fa-portrait"></i> Document <span class="badge badge-light">checkify</span>');
                    self.setContent('<span class="badge badge-light"><i class="fas fa-film"></i> Document Image</span>');
                    self.setContentAppend('<br><img id="agentOcrPicture" class="img-fluid investigation-actual-image" src="' + response.ocrData + '" /> ');
                    self.setContentAppend('<br><span class="badge badge-light"><i class="fas fa-info"></i> Image Scan Info</span> ');
                    self.setContentAppend('<br><i>' + response.qrData + '</i>');
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            }
        })
    })

    $('#policy-detail').click(function () {
        $.confirm({
            columnClass: 'medium',
            title: "Policy details",
            closeIcon: true,
            columnClass: 'medium',
            type: 'green',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    'type': 'GET',
                    'url': '/api/ClaimsInvestigation/GetPolicyDetail?id=' + $('#policyDetailId').val(),
                    'dataType': 'json',
                    'success': function (response) {
                        console.log(response);
                        self.setTitle('<i class="far fa-file-powerpoint"></i> Policy details ');
                        self.setContent('<article>');
                        self.setContent('<div class="bb-blog-inner">');

                        self.setContentAppend('<div class="card card-solid">');
                        self.setContentAppend('<header class="bb-blog-header">');
                        self.setContentAppend('<h5 class="bb-blog-title" itemprop="name">Policy #: ' + response.contractNumber);
                        self.setContentAppend('</header>');
                        self.setContentAppend('<div class="card-body pb-0">');
                        self.setContentAppend('<div class="row">');
                        self.setContentAppend('<b> Claim Type: </b>' + response.claimType);
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Insured Amount</b>: <i class="fas fa-rupee-sign"></i> ' + response.sumAssuredValue);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Policy Issue Date</b>: <i class="far fa-clock"></i> ' + response.contractIssueDate);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Incident Date</b>: <i class="far fa-clock"></i> ' + response.dateOfIncident);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Service Type</b>: <i class="	fas fa-tools"></i> ' + response.investigationServiceType);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Reason to verify</b>: <i class="fas fa-bolt"></i> ' + response.caseEnabler);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Cause of Incidence</b>: <i class="far fa-check-circle"></i> ' + response.causeOfLoss);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><p class="fa-li">');
                        self.setContentAppend('<b> Budget Centre</b>: <i class="far fa-check-circle"></i> ' + response.costCentre);
                        self.setContentAppend('</p');
                        self.setContentAppend('<br><b> Policy Doc</b>:');
                        self.setContentAppend('<br><img id="agentLocationPicture" class="img-fluid investigation-actual-image" src="' + response.document + '" /> ');
                        self.setContentAppend('</p');
                        self.setContentAppend('</div>');
                        self.setContentAppend('</div>');
                        self.setContentAppend('</div>');
                        self.setContentAppend('</div>');
                        self.setContentAppend('</article>');
                    }
                }, function () {
                    //This function is for unhover.
                });
            }
        })
    });

    $('#customer-detail').click(function () {
        $.confirm({
            columnClass: 'medium',
            title: "Customer detail",
            icon: 'fa fa-user-plus',
            closeIcon: true,
            columnClass: 'medium',
            type: 'green',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetCustomerDetail?id=' + $('#customerDetailId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    self.setContent('<hr>');
                    self.setContentAppend('<header>');
                    self.setContentAppend('<b>Customer Name</b>: ' + response.customerName);
                    self.setContentAppend('</header>');
                    self.setContentAppend('<br><b>Date of birth</b> : ' + response.dateOfBirth);
                    self.setContentAppend('<br><b>Occupation</b> : ' + response.occupation);
                    self.setContentAppend('<br><b>Income</b> : ' + response.income);
                    self.setContentAppend('<br><b>Education</b> : ' + response.education);
                    self.setContentAppend('<br><b>Address</b> : ' + response.address);
                    self.setContentAppend('<br><b>Phone</b> : ' + response.contactNumber);
                    self.setContentAppend('<br><img id="agentLocationPicture" class="img-fluid investigation-actual-image" src="' + response.customer + '" />');
                    self.setTitle('Customer detail');
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            }
        })
    })

    $('#beneficiary-detail').click(function () {
        $.confirm({
            columnClass: 'medium',
            title: "Beneficiary details",
            icon: 'fas fa-user-tie',
            closeIcon: true,
            columnClass: 'medium',
            type: 'green',
            buttons: {
                confirm: {
                    text: "Ok",
                    btnClass: 'btn-secondary',
                    action: function () {
                        askConfirmation = false;
                    }
                }
            },
            content: function () {
                var self = this;
                return $.ajax({
                    url: '/api/ClaimsInvestigation/GetBeneficiaryDetail?id=' + $('#beneficiaryId').val() + '&claimId=' + $('#claimId').val(),
                    dataType: 'json',
                    method: 'get'
                }).done(function (response) {
                    self.setContent('<header>');
                    self.setContentAppend('<hr>');
                    self.setContentAppend('<b>Beneficiary Name</b>: ' + response.beneficiaryName);
                    self.setContentAppend('</header>');
                    self.setContentAppend('<br><b>Relation</b> : ' + response.beneficiaryRelation);
                    self.setContentAppend('<br><b>Phone</b>: ' + response.contactNumber);
                    self.setContentAppend('<br><b>Income</b>: ' + response.income);
                    self.setContentAppend('<br><b>Address</b>: ' + response.address);
                    self.setContentAppend('<br><img id="agentLocationPicture" class="img-fluid investigation-actual-image" src="' + response.beneficiary + '" /> ');
                }).fail(function () {
                    self.setContent('Something went wrong.');
                });
            }
        })
    })

    $('#policy-comments').click(function () {
        $.confirm({
            title: 'Policy Note!!!',
            closeIcon: true,
            type: 'green',
            icon: 'far fa-file-powerpoint',
            content: '' +
                '<form action="" class="formName">' +
                '<div class="form-group">' +
                '<hr>' +
                '<label>Enter note on Policy</label>' +
                '<input type="text" placeholder="Enter note" class="name form-control" required />' +
                '</div>' +
                '</form>',
            buttons: {
                formSubmit: {
                    text: 'Add Note',
                    btnClass: 'btn-green',
                    action: function () {
                        var name = this.$content.find('.name').val();
                        if (!name) {
                            $.alert('Provide Policy note!!!');
                            return false;
                        }
                        $.alert('Policy note is ' + name);
                    }
                },
                cancel: function () {
                    //close
                },
            },
            onContentReady: function () {
                // bind to events
                var jc = this;
                this.$content.find('form').on('submit', function (e) {
                    // if the user submits the form by pressing enter in the field.
                    e.preventDefault();
                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                });
            }
        });
    })

    $('#customer-comments').click(function () {
        $.confirm({
            title: 'Customer Note!!!',
            closeIcon: true,
            type: 'green',
            icon: 'fa fa-user-plus',
            content: '' +
                '<form action="" class="formName">' +
                '<div class="form-group">' +
                '<hr>' +
                '<label>Enter note on Customer</label>' +
                '<input type="text" placeholder="Enter note" class="name form-control" required />' +
                '</div>' +
                '</form>',
            buttons: {
                formSubmit: {
                    text: 'Add Note',
                    btnClass: 'btn-green',
                    action: function () {
                        var name = this.$content.find('.name').val();
                        if (!name) {
                            $.alert('Provide Customer note!!!');
                            return false;
                        }
                        $.alert('Customer note is ' + name);
                    }
                },
                cancel: function () {
                    //close
                },
            },
            onContentReady: function () {
                // bind to events
                var jc = this;
                this.$content.find('form').on('submit', function (e) {
                    // if the user submits the form by pressing enter in the field.
                    e.preventDefault();
                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                });
            }
        });
    })

    $('#beneficiary-comments').click(function () {
        $.confirm({
            title: 'Beneficiary Note!!!',
            icon: 'fas fa-user-tie',
            closeIcon: true,
            type: 'green',
            content: '' +
                '<form action="" class="formName">' +
                '<div class="form-group">' +
                '<hr>' +
                '<label>Enter note about Beneficiary</label>' +
                '<input type="text" placeholder="Enter note" class="name form-control" required />' +
                '</div>' +
                '</form>',
            buttons: {
                formSubmit: {
                    text: 'Add Note',
                    btnClass: 'btn-green',
                    action: function () {
                        var name = this.$content.find('.name').val();
                        if (!name) {
                            $.alert('Provide Beneficiary note!!!');
                            return false;
                        }
                        $.alert('Beneficiary note is ' + name);
                    }
                },
                cancel: function () {
                    //close
                },
            },
            onContentReady: function () {
                // bind to events
                var jc = this;
                this.$content.find('form').on('submit', function (e) {
                    // if the user submits the form by pressing enter in the field.
                    e.preventDefault();
                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                });
            }
        });
    })
});

function checkIfAllChecked(elements) {
    var totalElmentCount = elements.length;
    var totalCheckedElements = elements.filter(":checked").length;
    return (totalElmentCount == totalCheckedElements)
}

function checkIfAnyChecked(elements) {
    var hasAnyCheckboxChecked = false;

    $.each(elements, function (index, element) {
        if (element.checked === true) {
            hasAnyCheckboxChecked = true;
        }
    });
    return hasAnyCheckboxChecked;
}

async function initPopMap(_position, title) {
    const { Map } = await google.maps.importLibrary("maps");
    // The location of Uluru
    var position = { lat: -25.344, lng: 131.031 };
    if (_position) {
        position = _position;
    }
    var element = document.getElementById("pop-face-map");
    // The map, centered at Uluru
    mapz = new Map(element, {
        scaleControl: true,
        zoom: 14,
        center: position,
        mapId: "4504f8b37365c3d0",
        mapTypeId: google.maps.MapTypeId.ROADMAP,
    });

    var marker = new google.maps.Marker({ position: position, map: mapz, title: title })
}

function loadState(obj, showDefaultOption = true) {
    var value = obj.value;
    $.post("/MasterData/GetStatesByCountryId", { countryId: value }, function (data) {
        PopulateStateDropDown("#PinCodeId", "#DistrictId", "#StateId", data, "<option>--- SELECT ---</option>", "<option>--- SELECT ---</option>", "<option>--- SELECT ---</option>", showDefaultOption);
    });
}
function loadDistrict(obj, showDefaultOption = true) {
    var value = obj.value;
    $.post("/MasterData/GetDistrictByStateId", { stateId: value }, function (data) {
        PopulateDistrictDropDown("#PinCodeId", "#DistrictId", data, "<option>--- SELECT ---</option>", "<option>--- SELECT ---</option>", showDefaultOption);
    });
}
function loadPinCode(obj, showDefaultOption = true) {
    var value = obj.value;
    $.post("/MasterData/GetPinCodesByDistrictId", { districtId: value }, function (data) {
        PopulatePinCodeDropDown("#PinCodeId", data, "<option>--- SELECT ---</option>", showDefaultOption);
    });
}

function enableSubmitButton(obj, showDefaultOption = true) {
    var value = obj.value;
    $('#create-pincode').prop('disabled', false);
}

function loadSubStatus(obj) {
    var value = obj.value;
    $.post("/InvestigationCaseSubStatus/GetSubstatusBystatusId", { InvestigationCaseStatusId: value }, function (data) {
        PopulateSubStatus("#InvestigationCaseSubStatusId", data, "<option>--- SELECT ---</option>");
    });
}

function PopulateSubStatus(dropDownId, list, option) {
    $(dropDownId).empty();
    $(dropDownId).append(option)
    $.each(list, function (index, row) {
        $(dropDownId).append("<option value='" + row.investigationServiceTypeId + "'>" + row.code + "</option>")
    });
}
let lobObj;
let investigationServiceObj;

function loadInvestigationServices(obj) {
    var value = obj.value;
    lobObj = value;
    localStorage.setItem('lobId', value);
    $.post("/VendorService/GetInvestigationServicesByLineOfBusinessId", { LineOfBusinessId: value }, function (data) {
        PopulateInvestigationServices("#InvestigationServiceTypeId", data, "<option>--- SELECT ---</option>");
    });
}

function setInvestigationServices(obj) {
    localStorage.setItem('serviceId', obj.value);
    investigationServiceObj = obj.value;
}
function PopulateInvestigationServices(dropDownId, list, option) {
    $(dropDownId).empty();
    $(dropDownId).append(option)
    $.each(list, function (index, row) {
        $(dropDownId).append("<option value='" + row.investigationServiceTypeId + "'>" + row.code + "</option>")
    });
}
function PopulateDistrictDropDown(pinCodedropDownId, districtDropdownId, list, pincodeOption, districtOption, showDefaultOption) {
    $(pinCodedropDownId).empty();
    $(pinCodedropDownId).append(pincodeOption)

    $(districtDropdownId).empty();
    $(districtDropdownId).append(districtOption)

    $.each(list, function (index, row) {
        $(districtDropdownId).append("<option value='" + row.districtId + "'>" + row.name + "</option>")
    });
}
function PopulatePinCodeDropDown(dropDownId, list, option, showDefaultOption) {
    $(dropDownId).empty();
    if (showDefaultOption)
        $(dropDownId).append(option)
    if (list && list.length > 0) {
        $.each(list, function (index, row) {
            $(dropDownId).append("<option value='" + row.pinCodeId + "'>" + row.name + " -- " + row.code + "</option>");
            $('#create-pincode').prop('disabled', false);
        });
    }
    else {
        $(dropDownId).append("<option value='-1'>NO - PINCODE - AVAILABLE</option>")
        $('#create-pincode').prop('disabled', true);
    }
}
function PopulateStateDropDown(pinCodedropDownId, districtDropDownId, stateDropDownId, list, stateOption, districtOption, pincodeOption, showDefaultOption) {
    $(stateDropDownId).empty();
    $(districtDropDownId).empty();
    $(pinCodedropDownId).empty();

    $(stateDropDownId).append(stateOption);
    $(districtDropDownId).append(districtOption);
    $(pinCodedropDownId).append(pincodeOption);

    $.each(list, function (index, row) {
        $(stateDropDownId).append("<option value='" + row.stateId + "'>" + row.name + "</option>")
    });
}
function toggleChecked(status) {
    $("#checkboxes input").each(function () {
        // Set the checked status of each to match the
        // checked status of the check all checkbox:
        $(this).prop("checked", status);
    });
}
function readURL(input) {
    var url = input.value;
    var ext = url.substring(url.lastIndexOf('.') + 1).toLowerCase();
    if (input.files && input.files[0] && (ext == "gif" || ext == "png" || ext == "jpeg" || ext == "jpg" || ext == "csv")) {
        var reader = new FileReader();

        reader.onload = function (e) {
            $('#img').attr('src', e.target.result);
        }

        reader.readAsDataURL(input.files[0]);
    } else {
        $('#img').attr('src', '/img/no-image.png');
    }
}

function loadRemainingPinCode(obj, showDefaultOption = true, caseId) {
    var value = obj.value;
    $.post("/MasterData/GetPincodesByDistrictIdWithoutPreviousSelected", { districtId: value, caseId: caseId }, function (data) {
        PopulatePinCodeDropDown("#PinCodeId", data, "<option>--SELECT PINCODE--</option>", showDefaultOption);
    });
}
function loadRemainingServicePinCode(obj, showDefaultOption = true, vendorId, lineId) {
    var value = obj.value;

    var lobId = localStorage.getItem('lobId');

    var serviceId = localStorage.getItem('serviceId');

    $.post("/MasterData/GetPincodesByDistrictIdWithoutPreviousSelectedService", { districtId: value, vendorId: vendorId, lobId: lobId, serviceId: serviceId }, function (data) {
        PopulatePinCodeDropDown("#PinCodeId", data, "<option>--SELECT PINCODE--</option>", showDefaultOption);
    });
}

function createCharts(container, txn, sum, titleText, totalspent) {
    Highcharts.chart(container, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'pie'
        },
        title: {
            text: titleText + ' ' + totalspent,
            style: {
                fontSize: '.9rem',
                fontFamily: 'Arial Narrow, sans-serif'
            }
        },
        xAxis: {
            type: 'category',
            labels: {
                rotation: -45,
                style: {
                    fontSize: '12px',
                    fontFamily: 'Arial Narrow, sans-serif'
                }
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: txn + ' Count'
            }
        },
        legend: {
            enabled: false
        },
        tooltip: {
            pointFormat: 'Total ' + txn + ': Count <b>{point.y} </b>'
        },
        series: [{
            type: 'pie',
            data: sum,
        }]
    });
}
function createChartColumn(container, txn, sum, titleText, totalspent) {
    Highcharts.chart(container, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'column'
        },
        title: {
            text: titleText + ' ' + totalspent,
            style: {
                fontSize: '.9rem',
                fontFamily: 'Arial Narrow, sans-serif'
            }
        },
        xAxis: {
            type: 'category',
            labels: {
                rotation: -45,
                style: {
                    fontSize: '12px',
                    fontFamily: 'Arial Narrow, sans-serif'
                }
            }
        },
        yAxis: {
            min: 0,
            title: {
                text: txn + ' Count'
            }
        },
        legend: {
            enabled: false
        },
        tooltip: {
            pointFormat: 'Total ' + txn + ': Count <b>{point.y} </b>'
        },
        series: [{
            type: 'column',
            data: sum,
        }]
    });
}
function createMonthChart(container, titleText, data, keys, total) {
    Highcharts.chart(container, {
        credits: {
            enabled: false
        },
        chart: {
            marginRight: 0
        },
        title: {
            text: 'Total ' + titleText + ' Count ' + total,
            style: {
                fontSize: '1rem',
                fontFamily: 'Arial Narrow, sans-serif'
            }
        },
        legend: {
            enabled: false
        },
        xAxis: {
            categories: keys
        },
        yAxis: {
            min: 0,
            title: {
                text: ' Count'
            }
        },
        series: [{
            data: data,
            color: 'green'
        }]
    });
}

function GetChart(title, url, container) {
    var titleMessage = "Last 12 month " + title + ":Count";
    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var weeklydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    weeklydata.push(arrL);
                }
                createMonthChart(container, title, weeklydata, keys, totalspent);
            }
        }
    })
}

function GetWeekly(title, url, container) {
    var titleMessage = "All Current " + title + ":Grouped by status";
    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var weeklydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    weeklydata.push(arrL);
                }
                createChartColumn(container, title, weeklydata, titleMessage, totalspent);
            }
        }
    })
}
function GetWeeklyPie(title, url, container) {
    var titleMessage = "All Current " + title + ":Count";
    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var weeklydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    weeklydata.push(arrL);
                }
                createCharts(container, title, weeklydata, titleMessage, totalspent);
            }
        }
    })
}

function GetMonthly(title, url, container) {
    var titleMessage = "All Current " + title + "Count by status";

    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var monthlydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    monthlydata.push(arrL);
                }
                createChartColumn(container, title, monthlydata, titleMessage, totalspent);
            }
        }
    })
}
function GetMonthlyPie(title, url, container) {
    var titleMessage = "All Current " + title + "Count by status";

    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var monthlydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    monthlydata.push(arrL);
                }
                createCharts(container, title, monthlydata, titleMessage, totalspent);
            }
        }
    })
}

function createChartTat(container, txn, sum, titleText, totalspent) {
    Highcharts.chart(container, {
        credits: {
            enabled: false
        },
        chart: {
            type: 'column'
        },
        title: {
            text: titleText + ' ' + totalspent,
            style: {
                fontSize: '.9rem',
                fontFamily: 'Arial Narrow, sans-serif'
            }
        },
        xAxis: {
            categories: ['0 Day', '1 Day', '2 Day', '3 Day', '4 Day', '5 plus Day']
        },
        yAxis: {
            min: 0,
            title: {
                text: txn + ' Status changes '
            }
        },
        legend: {
            enabled: true
        },
        tooltip: {
            pointFormat: 'Total ' + txn + ': Status changes <b>{point.y} </b>'
        },
        series: sum
    });
}
function GetWeeklyTat(title, url, container) {
    var titleMessage = "All Current " + title + ":Status changes";
    $.ajax({
        type: "GET",
        url: "/Dashboard/" + url,
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result) {
                var keys = Object.keys(result);
                var weeklydata = new Array();
                var totalspent = 0.0;
                for (var i = 0; i < keys.length; i++) {
                    var arrL = new Array();
                    arrL.push(keys[i]);
                    arrL.push(result[keys[i]]);
                    totalspent += result[keys[i]];
                    weeklydata.push(arrL);
                }
                createChartTat(container, title, result.tatDetails, titleMessage, result.count);
            }
        }
    })
}