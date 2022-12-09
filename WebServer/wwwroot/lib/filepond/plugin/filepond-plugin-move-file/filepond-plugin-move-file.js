/*!
 * FilePondPluginGetFile 1.0.6
 * Licensed under MIT, https://opensource.org/licenses/MIT/
 * Please visit undefined for details.
 */

/* eslint-disable */

(function (global, factory) {
  typeof exports === 'object' && typeof module !== 'undefined'
    ? (module.exports = factory())
    : typeof define === 'function' && define.amd
    ? define(factory)
    : ((global =
        typeof globalThis !== 'undefined' ? globalThis : global || self),
      (global.FilePondPluginMoveFile = factory()));
})(this, function () {
  'use strict';

    /**
    * Register the moveup component by inserting the moveup icon
    */
    const registerMoveComponent = (
        item,
        root,
        labelButtonMoveup,
        labelButtonMovedown,
        labelButtonPreview,
    ) => {
        const info = root.element.querySelector('.filepond--file-info-main');

        if (!root.query('GET_DISABLED') && root.query('GET_ALLOW_MULTIPLE')) {
            const moveupIcon = getMoveupIcon(labelButtonMoveup);
            info.prepend(moveupIcon);
            moveupIcon.addEventListener('click', () => {
                //let curFiles = root.query('GET_ACTIVE_ITEMS');
                let curFiles = root.query('GET_ITEMS');
                let curIndex = -1;
                for (let i = 0; i < curFiles.length; i++) {
                    if (curFiles[i].id === item.id) {
                        curIndex = i;
                        break;
                    }
                }
                if (curIndex === 0)
                    return;

                const tmpFile = curFiles[curIndex];
                curFiles[curIndex] = curFiles[curIndex - 1];
                curFiles[curIndex - 1] = tmpFile;

                root.query('DID_SORT_ITEMS', {
                    items: curFiles,
                });

                const $fieldset = $(root.element).parent().parent().parent().parent().parent().find('.filepond--data');
                let filedatas = [];
                for (let i = 0; i < curFiles.length; i++) {
                    filedatas.push($fieldset.find(`input[type="hidden"][value="${curFiles[i].serverId}"]`));
                }
                $fieldset.empty();
                for (let i = 0; i < filedatas.length; i++) {
                    $fieldset.append(filedatas[i]);
                }
            });

            const movedownIcon = getMovedownIcon(labelButtonMovedown);
            info.prepend(movedownIcon);
            movedownIcon.addEventListener('click', () => {
                //let curFiles = root.query('GET_ACTIVE_ITEMS');
                let curFiles = root.query('GET_ITEMS');
                let curIndex = -1;
                for (let i = 0; i < curFiles.length; i++) {
                    if (curFiles[i].id === item.id) {
                        curIndex = i;
                        break;
                    }
                }
                if (curIndex === curFiles.length - 1)
                    return;

                const tmpFile = curFiles[curIndex];
                curFiles[curIndex] = curFiles[curIndex + 1];
                curFiles[curIndex + 1] = tmpFile;

                root.query('DID_SORT_ITEMS', {
                    items: curFiles,
                });

                const $fieldset = $(root.element).parent().parent().parent().parent().parent().find('.filepond--data');
                let filedatas = [];
                for (let i = 0; i < curFiles.length; i++) {
                    filedatas.push($fieldset.find(`input[type="hidden"][value="${curFiles[i].serverId}"]`));
                }
                $fieldset.empty();
                for (let i = 0; i < filedatas.length; i++) {
                    $fieldset.append(filedatas[i]);
                }
            });
        }

        //const previewIcon = getPreviewIcon(labelButtonPreview);
        //info.prepend(previewIcon);
        //previewIcon.addEventListener('click', (e) => {
        //    const url = `/api/File/Download/${item.serverId}`;
        //    if (item.fileType.toLowerCase().indexOf('image') >= 0) {
        //        Swal.fire({
        //            'title': item.filename,
        //            'width': 800,
        //            'html': `<img width="600" src="${url}"></img>`,
        //            didOpen: () => {
        //                Swal.imgUrl = url;
        //                onImageLoaded(url);
        //            }
        //        });
        //    }
        //    else if (item.fileType.toLowerCase().indexOf('video') >= 0) {
        //        Swal.fire({
        //            'title': item.filename,
        //            'width': 800,
        //            'html': '<video width="600" controls><source src="' + url + '" type="' + item.filetype + '"></video>'
        //        })
        //    }
        //    else if (item.fileType.toLowerCase().indexOf('audio') >= 0) {
        //        Swal.fire({
        //            'title': item.filename,
        //            'width': 800,
        //            'html': '<audio width="600" controls><source src="' + url + '" type="' + item.filetype + '"></audio>'
        //        })
        //    }
        //    else {
        //        OpenWin4Viewer(e, item.serverId);
        //    }
        //});
    };
    /**
    * Generates the moveup icon
    */
    const getMoveupIcon = (labelButton) => {
        let icon = document.createElement('span');
        icon.className = 'filepond--moveup-icon';
        icon.title = labelButton;
        return icon;
    };
    const getMovedownIcon = (labelButton) => {
        let icon = document.createElement('span');
        icon.className = 'filepond--movedown-icon';
        icon.title = labelButton;
        return icon;
    };
    const getPreviewIcon = (labelButton) => {
        let icon = document.createElement('span');
        icon.className = 'filepond--preview-icon';
        icon.title = labelButton;
        return icon;
    };

    /**
    * Moveup Plugin
    */
    const plugin = (fpAPI) => {
    const { addFilter, utils } = fpAPI;
    const { Type, createRoute } = utils; // called for each view that is created right after the 'create' method

    addFilter('CREATE_VIEW', (viewAPI) => {
        // get reference to created view
        const { is, view, query } = viewAPI; // only hook up to item view

        if (!is('file')) {
            return;
        } // create the get file plugin

        const didLoadItem = ({ root, props }) => {
            const { id } = props;
            const item = query('GET_ITEM', id);

            if (!item || item.archived) {
                return;
            }

            registerMoveComponent(
                item,
                root,
                root.query('GET_LABEL_BUTTON_MOVEUP_ITEM'),
                root.query('GET_LABEL_BUTTON_MOVEDOWN_ITEM'),
                root.query('GET_LABEL_BUTTON_PREVIEW_ITEM'),
            );
        }; // start writing

        view.registerWriter(
            createRoute(
                {
                    DID_LOAD_ITEM: didLoadItem,
                },
                ({ root, props }) => {
                    const { id } = props;
                    const item = query('GET_ITEM', id); // don't do anything while hidden

                    if (root.rect.element.hidden) return;
                }
            )
        );
    }); // expose plugin

    return {
        options: {
            labelButtonMoveupItem: ['Moveup file', Type.STRING],
            labelButtonMovedownItem: ['Movedown file', Type.STRING],
            labelButtonPreviewItem: ['Preview file', Type.STRING],
        },
    };
    }; // fire pluginloaded event if running in browser, this allows registering the plugin when using async script tags

    const isBrowser =
    typeof window !== 'undefined' && typeof window.document !== 'undefined';

    if (isBrowser) {
        document.dispatchEvent(
            new CustomEvent('FilePond:pluginloaded', {
            detail: plugin,
            })
        );
    }

    return plugin;
});
