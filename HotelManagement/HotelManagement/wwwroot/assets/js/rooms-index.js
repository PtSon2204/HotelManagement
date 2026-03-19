$('#deleteModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var id = button.data('id');
    var name = button.data('name');

    $('#deleteRoomNumber').text(name);
    $('#deleteForm').attr('action', '/Admin/Rooms/Delete/' + id);
});
