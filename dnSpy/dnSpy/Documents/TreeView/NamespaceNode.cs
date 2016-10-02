﻿/*
    Copyright (C) 2014-2016 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using dnlib.DotNet;
using dnSpy.Contracts.Decompiler;
using dnSpy.Contracts.Documents.TreeView;
using dnSpy.Contracts.Images;
using dnSpy.Contracts.Text;
using dnSpy.Contracts.TreeView;

namespace dnSpy.Documents.TreeView {
	sealed class NamespaceNode : DocumentTreeNodeData, INamespaceNode {
		public override Guid Guid => new Guid(DocumentTreeViewConstants.NAMESPACE_NODE_GUID);
		protected override ImageReference GetIcon(IDotNetImageService dnImgMgr) =>
			dnImgMgr.GetNamespaceImageReference();
		public string Name { get; set; }
		public override NodePathName NodePathName => new NodePathName(Guid, Name);
		public override ITreeNodeGroup TreeNodeGroup { get; }

		public NamespaceNode(ITreeNodeGroup treeNodeGroup, string name, List<TypeDef> types) {
			this.TreeNodeGroup = treeNodeGroup;
			this.Name = name;
			this.typesToCreate = types;
		}

		public override void Initialize() => TreeNode.LazyLoading = true;

		public override IEnumerable<ITreeNodeData> CreateChildren() {
			foreach (var type in typesToCreate)
				yield return new TypeNode(Context.DocumentTreeView.DocumentTreeNodeGroups.GetGroup(DocumentTreeNodeGroupType.TypeTreeNodeGroupNamespace), type);
			typesToCreate = null;
		}
		List<TypeDef> typesToCreate;

		protected override void Write(ITextColorWriter output, IDecompiler decompiler) =>
			new NodePrinter().WriteNamespace(output, decompiler, Name);
		public ITypeNode Create(TypeDef type) => Context.DocumentTreeView.Create(type);

		public override FilterType GetFilterType(IDocumentTreeNodeFilter filter) {
			var p = TreeNode.Parent;
			var parent = p == null ? null : p.Data as IModuleDocumentNode;
			Debug.Assert(parent != null);
			if (parent == null)
				return FilterType.Default;
			var res = filter.GetResult(Name, parent.Document);
			if (res.FilterType != FilterType.Default)
				return res.FilterType;
			return FilterType.CheckChildren;
		}
	}
}