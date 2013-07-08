(function (document, console, $, undefined) {
    $(document).ready(function () {
        var guitarsUrl = '/guitars',
            guitars = document.getElementById('guitars');

        function createListItem(name, url) {
            var li = document.createElement('li'),
                content = document.createElement('a'),
                deleteButton = document.createElement('button');
            content.setAttribute('href', url);
            content.textContent = name;
            li.appendChild(content);
            deleteButton.setAttribute('class', 'delete btn btn-mini');
            deleteButton.setAttribute('data-href', url);
            deleteButton.setAttribute('type', 'button');
            deleteButton.textContent = 'Delete';
            li.appendChild(deleteButton);
            li.setAttribute('data-href', url);
            return li;
        }

        $.get(guitarsUrl, function (data) {
            for (var i = 0, len = data.length; i < len; i++) {
                var guitar = data[i],
                    li = createListItem(guitar.name, guitar.link);
                guitars.appendChild(li);
            }
        });

        $('form button[type="submit"]').on('click', function (ev) {
            ev.preventDefault();
            var name = document.getElementById('name').value,
                formData = { name: name };
            $.ajax({
                type: 'POST',
                url: guitarsUrl,
                contentType: 'application/json',
                data: JSON.stringify(formData)
            }).
            done(function (data, status, request) {
                var url = request.getResponseHeader('Location'),
                    li = createListItem(data.name, url);
                guitars.appendChild(li);
            }).
            fail(function (request, status) {
                console.log('Failed with status: ' + status);
            });
        });

        $(document).on('click', 'button.delete', function (ev) {
            var $this = $(this),
                url = $this.data('href');
            ev.preventDefault();
            $.ajax({
                url: url,
                type: 'DELETE',
                context: this
            }).
            done(function () {
                var li = this.parentNode,
                    list = li.parentNode;
                list.removeChild(li);
            }).
            fail(function (request, status) {
                console.log('Failed with status: ' + status);
            });
        });
    });
})(document, console, jQuery);
